using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using NBitcoin.RPC;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unchain.Crypto;
using Unchain.Crypto.ZeroKnowledge;
using UnchainexWallet.Backend.Controllers;
using UnchainexWallet.Cache;
using UnchainexWallet.Coordinator.Controllers;
using UnchainexWallet.Crypto;
using UnchainexWallet.Crypto.Randomness;
using UnchainexWallet.Extensions;
using UnchainexWallet.Helpers;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Backend.Statistics;
using UnchainexWallet.Unchain.Client;
using UnchainexWallet.Unchain.Models;
using UnchainexWallet.Unchain.Models.MultipartyTransaction;
using Xunit;
using UnchainexWallet.Unchain.Client.CoinJoin.Client;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Client;

public class ArenaClientTests
{
	[Fact]
	public async Task FullP2wpkhCoinjoinTestAsync()
	{
		await TestFullCoinjoinAsync(ScriptPubKeyType.Segwit, Constants.P2wpkhInputVirtualSize);
	}

	[Fact]
	public async Task FullP2trCoinjoinTestAsync()
	{
		await TestFullCoinjoinAsync(ScriptPubKeyType.TaprootBIP86, Constants.P2trInputVirtualSize);
	}

	[Fact]
	public async Task RemoveInputAsyncTestAsync()
	{
		var config = new UnchainConfig();
		var round = UnchainFactory.CreateRound(config);
		round.SetPhase(Phase.ConnectionConfirmation);
		var fundingTx = BitcoinFactory.CreateSmartTransaction(ownOutputCount: 1);
		var coin = fundingTx.WalletOutputs.First().Coin;
		var alice = new Alice(coin, new OwnershipProof(), round, Guid.NewGuid());
		round.Alices.Add(alice);

		using Arena arena = await ArenaBuilder.From(config).CreateAndStartAsync(round);

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var idempotencyRequestCache = new IdempotencyRequestCache(memoryCache);
		using CoinJoinFeeRateStatStore coinJoinFeeRateStatStore = new("FeeRateStatSore.txt", config, arena.Rpc);
		var unChainApi = new UnchainController(idempotencyRequestCache, arena, coinJoinFeeRateStatStore);

		var apiClient = new ArenaClient(null!, null!, config.CoordinatorIdentifier, unChainApi);

		round.SetPhase(Phase.InputRegistration);

		await apiClient.RemoveInputAsync(round.Id, alice.Id, CancellationToken.None);
		Assert.Empty(round.Alices);
	}

	[Fact]
	public async Task SignTransactionAsync()
	{
		UnchainConfig config = new();
		Round round = UnchainFactory.CreateRound(config);
		var password = "satoshi";

		var km = ServiceFactory.CreateKeyManager(password);
		var keyChain = new KeyChain(km, password);
		var destinationProvider = new InternalDestinationProvider(km);

		var coins = destinationProvider.GetNextDestinations(2, false)
			.Select(destination => (
				Coin: new Coin(BitcoinFactory.CreateOutPoint(), new TxOut(Money.Coins(1.0m), destination)),
				OwnershipProof: keyChain.GetOwnershipProof(destination, UnchainFactory.CreateCommitmentData(round.Id))))
			.ToArray();

		Alice alice1 = UnchainFactory.CreateAlice(coins[0].Coin, coins[0].OwnershipProof, round: round);
		round.Alices.Add(alice1);

		Alice alice2 = UnchainFactory.CreateAlice(coins[1].Coin, coins[1].OwnershipProof, round: round);
		round.Alices.Add(alice2);

		using Arena arena = await ArenaBuilder.From(config).CreateAndStartAsync(round);

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var idempotencyRequestCache = new IdempotencyRequestCache(memoryCache);

		using CoinJoinFeeRateStatStore coinJoinFeeRateStatStore = new("FeeRateStatSore.txt", config, arena.Rpc);
		var unChainApi = new UnchainController(idempotencyRequestCache, arena, coinJoinFeeRateStatStore);

		InsecureRandom rnd = InsecureRandom.Instance;
		var amountClient = new UnchainClient(round.AmountCredentialIssuerParameters, rnd, 4300000000000L);
		var vsizeClient = new UnchainClient(round.VsizeCredentialIssuerParameters, rnd, 2000L);
		var apiClient = new ArenaClient(amountClient, vsizeClient, config.CoordinatorIdentifier, unChainApi);

		round.SetPhase(Phase.TransactionSigning);

		var emptyState = round.Assert<ConstructionState>();
		var commitmentData = UnchainFactory.CreateCommitmentData(round.Id);

		// We can't use ``emptyState.Finalize()` because this is not a valid transaction so we fake it
		var finalizedEmptyState = new SigningState(round.Parameters, emptyState.Events);

		// No inputs in the coinjoin.
		await Assert.ThrowsAsync<ArgumentException>(async () =>
				await apiClient.SignTransactionAsync(round.Id, alice1.Coin, keyChain, finalizedEmptyState.CreateUnsignedTransactionWithPrecomputedData(), CancellationToken.None));

		var oneInput = emptyState.AddInput(alice1.Coin, alice1.OwnershipProof, commitmentData).Finalize();
		round.CoinjoinState = oneInput;

		// Trying to sign coins those are not in the coinjoin.
		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
				await apiClient.SignTransactionAsync(round.Id, alice2.Coin, keyChain, oneInput.CreateUnsignedTransactionWithPrecomputedData(), CancellationToken.None));

		var twoInputs = emptyState
			.AddInput(alice1.Coin, alice1.OwnershipProof, commitmentData)
			.AddInput(alice2.Coin, alice2.OwnershipProof, commitmentData)
			.Finalize();
		round.CoinjoinState = twoInputs;

		Assert.False(round.Assert<SigningState>().IsFullySigned);
		var unsigned = round.Assert<SigningState>().CreateUnsignedTransactionWithPrecomputedData();

		await apiClient.SignTransactionAsync(round.Id, alice1.Coin, keyChain, unsigned, CancellationToken.None);
		Assert.True(round.Assert<SigningState>().IsInputSigned(alice1.Coin.Outpoint));
		Assert.False(round.Assert<SigningState>().IsInputSigned(alice2.Coin.Outpoint));

		Assert.False(round.Assert<SigningState>().IsFullySigned);

		await apiClient.SignTransactionAsync(round.Id, alice2.Coin, keyChain, unsigned, CancellationToken.None);
		Assert.True(round.Assert<SigningState>().IsInputSigned(alice2.Coin.Outpoint));

		Assert.True(round.Assert<SigningState>().IsFullySigned);
	}

	private async Task TestFullCoinjoinAsync(ScriptPubKeyType scriptPubKeyType, int inputVirtualSize)
	{
		var config = new UnchainConfig { MaxInputCountByRound = 1, AllowP2trInputs = true, AllowP2trOutputs = true };
		var round = UnchainFactory.CreateRound(UnchainFactory.CreateRoundParameters(config));
		using var key = new Key();
		var outpoint = BitcoinFactory.CreateOutPoint();
		var mockRpc = UnchainFactory.CreatePreconfiguredRpcClient();
		mockRpc.OnGetTxOutAsync = (_, _, _) =>
			new GetTxOutResponse
			{
				IsCoinBase = false,
				Confirmations = 200,
				TxOut = new TxOut(Money.Coins(1m), key.PubKey.GetAddress(scriptPubKeyType, Network.Main)),
			};
		mockRpc.OnEstimateSmartFeeAsync = (_, _) =>
			Task.FromResult(new EstimateSmartFeeResponse
			{
				Blocks = 1000,
				FeeRate = new FeeRate(10m)
			});
		mockRpc.OnGetMempoolInfoAsync = () =>
			Task.FromResult(new MemPoolInfo
			{
				MinRelayTxFee = 1
			});
		mockRpc.OnGetRawTransactionAsync = (_, _) =>
			Task.FromResult(BitcoinFactory.CreateTransaction());

		using Arena arena = await ArenaBuilder.From(config).With(mockRpc).CreateAndStartAsync(round);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromMinutes(1));

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var idempotencyRequestCache = new IdempotencyRequestCache(memoryCache);

		using CoinJoinFeeRateStatStore coinJoinFeeRateStatStore = new("FeeRateStatSore.txt", config, arena.Rpc);
		var unChainApi = new UnchainController(idempotencyRequestCache, arena, coinJoinFeeRateStatStore);

		var roundState = RoundState.FromRound(round);
		var aliceArenaClient = new ArenaClient(
			roundState.CreateAmountCredentialClient(InsecureRandom.Instance),
			roundState.CreateVsizeCredentialClient(InsecureRandom.Instance),
			config.CoordinatorIdentifier,
			unChainApi);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id, scriptPubKeyType);

		var inputRegistrationResponseTask = aliceArenaClient.RegisterInputAsync(round.Id, outpoint, ownershipProof, CancellationToken.None);

		var amountsToRequest = new[]
		{
			Money.Coins(.75m) - round.Parameters.MiningFeeRate.GetFee(inputVirtualSize),
			Money.Coins(.25m),
		}.Select(x => x.Satoshi).ToArray();

		using var destinationKey1 = new Key();
		using var destinationKey2 = new Key();
		var scriptSize = (long)destinationKey1.PubKey.GetScriptPubKey(scriptPubKeyType).EstimateOutputVsize();

		var vsizesToRequest = new[] { round.Parameters.MaxVsizeAllocationPerAlice - (inputVirtualSize + (2 * scriptSize)), 2 * scriptSize };

		// Phase: Input Registration
		Assert.Equal(Phase.InputRegistration, round.Phase);

		var inputRegistrationResponse = await inputRegistrationResponseTask;
		var aliceId = inputRegistrationResponse.Value;
		var connectionConfirmationResponse1Task = aliceArenaClient.ConfirmConnectionAsync(
			round.Id,
			aliceId,
			amountsToRequest,
			vsizesToRequest,
			inputRegistrationResponse.IssuedAmountCredentials,
			inputRegistrationResponse.IssuedVsizeCredentials,
			CancellationToken.None);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromMinutes(1));
		Assert.Equal(Phase.ConnectionConfirmation, round.Phase);

		// Phase: Connection Confirmation
		var connectionConfirmationResponse1 = await connectionConfirmationResponse1Task;
		var connectionConfirmationResponse2Task = aliceArenaClient.ConfirmConnectionAsync(
			round.Id,
			aliceId,
			amountsToRequest,
			vsizesToRequest,
			connectionConfirmationResponse1.IssuedAmountCredentials,
			connectionConfirmationResponse1.IssuedVsizeCredentials,
			CancellationToken.None);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromMinutes(1));

		// Phase: Output Registration
		Assert.Equal(Phase.OutputRegistration, round.Phase);

		var bobArenaClient = new ArenaClient(
			roundState.CreateAmountCredentialClient(InsecureRandom.Instance),
			roundState.CreateVsizeCredentialClient(InsecureRandom.Instance),
			config.CoordinatorIdentifier,
			unChainApi);

		var connectionConfirmationResponse2 = await connectionConfirmationResponse2Task;
		var reissuanceResponse = await bobArenaClient.ReissueCredentialAsync(
			round.Id,
			amountsToRequest,
			Enumerable.Repeat(scriptSize, 2),
			connectionConfirmationResponse2.IssuedAmountCredentials.Take(ProtocolConstants.CredentialNumber),
			connectionConfirmationResponse2.IssuedVsizeCredentials.Skip(1).Take(ProtocolConstants.CredentialNumber), // first amount is the leftover value
			CancellationToken.None);

		Credential amountCred1 = reissuanceResponse.IssuedAmountCredentials.ElementAt(0);
		Credential amountCred2 = reissuanceResponse.IssuedAmountCredentials.ElementAt(1);
		Credential zeroAmountCred1 = reissuanceResponse.IssuedAmountCredentials.ElementAt(2);
		Credential zeroAmountCred2 = reissuanceResponse.IssuedAmountCredentials.ElementAt(3);

		Credential vsizeCred1 = reissuanceResponse.IssuedVsizeCredentials.ElementAt(0);
		Credential vsizeCred2 = reissuanceResponse.IssuedVsizeCredentials.ElementAt(1);
		Credential zeroVsizeCred1 = reissuanceResponse.IssuedVsizeCredentials.ElementAt(2);
		Credential zeroVsizeCred2 = reissuanceResponse.IssuedVsizeCredentials.ElementAt(3);

		var bobOutputRegistration1 = bobArenaClient.RegisterOutputAsync(
			round.Id,
			destinationKey1.PubKey.GetScriptPubKey(scriptPubKeyType),
			new[] { amountCred1, zeroAmountCred1 },
			new[] { vsizeCred1, zeroVsizeCred1 },
			CancellationToken.None);

		var bobOutputRegistration2 = bobArenaClient.RegisterOutputAsync(
			round.Id,
			destinationKey2.PubKey.GetScriptPubKey(scriptPubKeyType),
			new[] { amountCred2, zeroAmountCred2 },
			new[] { vsizeCred2, zeroVsizeCred2 },
			CancellationToken.None);

		await Task.WhenAll(bobOutputRegistration1, bobOutputRegistration2);
		await aliceArenaClient.ReadyToSignAsync(round.Id, aliceId, CancellationToken.None);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromMinutes(1));
		Assert.Equal(Phase.TransactionSigning, round.Phase);

		var tx = round.Assert<SigningState>().CreateTransaction();
		Assert.Single(tx.Inputs);
		Assert.Equal(2, tx.Outputs.Count);
	}
}
