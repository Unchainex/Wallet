using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Controllers;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Cache;
using UnchainexWallet.Coordinator.Controllers;
using UnchainexWallet.Crypto.Randomness;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Backend.Statistics;
using UnchainexWallet.Unchain.Client;
using UnchainexWallet.Unchain.Client.CoinJoin.Client;
using UnchainexWallet.Unchain.Client.RoundStateAwaiters;
using UnchainexWallet.Unchain.Models;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Client;

public class BobClientTests
{
	private TimeSpan TestTimeout { get; } = TimeSpan.FromMinutes(3);

	[Fact]
	public async Task RegisterOutputTestAsync()
	{
		using CancellationTokenSource cancellationTokenSource = new(TestTimeout);
		var token = cancellationTokenSource.Token;

		var config = new UnchainConfig { MaxInputCountByRound = 1 };
		var round = UnchainFactory.CreateRound(config);
		var km = ServiceFactory.CreateKeyManager("");
		var key = BitcoinFactory.CreateHdPubKey(km);
		SmartCoin coin1 = BitcoinFactory.CreateSmartCoin(key, Money.Coins(2m));

		var mockRpc = UnchainFactory.CreatePreconfiguredRpcClient(coin1.Coin);
		using Arena arena = await ArenaBuilder.From(config).With(mockRpc).CreateAndStartAsync(round);
		await arena.TriggerAndWaitRoundAsync(token);

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var idempotencyRequestCache = new IdempotencyRequestCache(memoryCache);

		using CoinJoinFeeRateStatStore coinJoinFeeRateStatStore = new("FeeRateStatSore.txt", config, arena.Rpc);
		var unChainApi = new UnchainController(idempotencyRequestCache, arena, coinJoinFeeRateStatStore);

		InsecureRandom insecureRandom = InsecureRandom.Instance;
		var roundState = RoundState.FromRound(round);
		var aliceArenaClient = new ArenaClient(
			roundState.CreateAmountCredentialClient(insecureRandom),
			roundState.CreateVsizeCredentialClient(insecureRandom),
			config.CoordinatorIdentifier,
			unChainApi);
		var bobArenaClient = new ArenaClient(
			roundState.CreateAmountCredentialClient(insecureRandom),
			roundState.CreateVsizeCredentialClient(insecureRandom),
			config.CoordinatorIdentifier,
			unChainApi);
		Assert.Equal(Phase.InputRegistration, round.Phase);

		using RoundStateUpdater roundStateUpdater = new(TimeSpan.FromSeconds(2), unChainApi);
		await roundStateUpdater.StartAsync(token);

		var keyChain = new KeyChain(km,"");
		var task = AliceClient.CreateRegisterAndConfirmInputAsync(RoundState.FromRound(round), aliceArenaClient, coin1, keyChain, roundStateUpdater, token, token, token);

		do
		{
			await arena.TriggerAndWaitRoundAsync(token);
		}
		while (round.Phase != Phase.ConnectionConfirmation);

		var aliceClient = await task;

		await arena.TriggerAndWaitRoundAsync(token);
		Assert.Equal(Phase.OutputRegistration, round.Phase);

		using var destinationKey = new Key();
		var destination = destinationKey.PubKey.GetScriptPubKey(ScriptPubKeyType.Segwit);

		var bobClient = new BobClient(round.Id, bobArenaClient);

		await bobClient.RegisterOutputAsync(
			destination,
			aliceClient.IssuedAmountCredentials.Take(ProtocolConstants.CredentialNumber),
			aliceClient.IssuedVsizeCredentials.Take(ProtocolConstants.CredentialNumber),
			token);

		var bob = Assert.Single(round.Bobs);
		Assert.Equal(destination, bob.Script);

		var credentialAmountSum = aliceClient.IssuedAmountCredentials.Take(ProtocolConstants.CredentialNumber).Sum(x => x.Value);
		Assert.Equal(credentialAmountSum, bob.CredentialAmount);
	}
}
