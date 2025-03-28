using NBitcoin;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Crypto;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.DoSPrevention;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Models;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PostRequests;

public class RegisterInputFailureTests
{
	private static async Task RegisterAndAssertWrongPhaseAsync(InputRegistrationRequest req, Arena handler)
	{
		var ex = await Assert.ThrowsAsync<WrongPhaseException>(async () => await handler.RegisterInputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.WrongPhase, ex.ErrorCode);
	}

	[Fact]
	public async Task RoundNotFoundAsync()
	{
		using Key key = new();
		using Arena arena = await ArenaBuilder.Default.CreateAndStartAsync();
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));

		var ownershipProof = UnchainFactory.CreateOwnershipProof(key);
		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(uint256.Zero, BitcoinFactory.CreateOutPoint(), ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.RoundNotFound, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task WrongPhaseAsync()
	{
		UnchainConfig cfg = new();
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync();
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		var round = arena.Rounds.First();
		var req = UnchainFactory.CreateInputRegistrationRequest(round, key: key, coin.Outpoint);

		foreach (Phase phase in Enum.GetValues(typeof(Phase)))
		{
			if (phase != Phase.InputRegistration)
			{
				round.SetPhase(phase);
				await RegisterAndAssertWrongPhaseAsync(req, arena);
			}
		}

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputRegistrationFullAsync()
	{
		UnchainConfig cfg = new() { MaxInputCountByRound = 3 };
		var round = UnchainFactory.CreateRound(cfg);
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);

		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		round.Alices.Add(UnchainFactory.CreateAlice(round));

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<WrongPhaseException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.WrongPhase, ex.ErrorCode);
		Assert.Equal(Phase.InputRegistration, round.Phase);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputRegistrationTimedOutAsync()
	{
		UnchainConfig cfg = new() { StandardInputRegistrationTimeout = TimeSpan.Zero };
		var round = UnchainFactory.CreateRound(cfg);
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync();

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));

		arena.Rounds.Add(round);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<WrongPhaseException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.WrongPhase, ex.ErrorCode);
		Assert.Equal(Phase.InputRegistration, round.Phase);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputBannedAsync()
	{
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);

		UnchainConfig cfg = UnchainFactory.CreateUnchainConfig();
		var round = UnchainFactory.CreateRound(cfg);

		Prison prison = UnchainFactory.CreatePrison();
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg, rpc, prison).CreateAndStartAsync(round);
		prison.FailedToSign(coin.Outpoint, Money.Coins(1m), round.Id);

		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));

		Assert.Equal(UnchainProtocolErrorCode.InputBanned, ex.ErrorCode);
		Assert.IsAssignableFrom<InputBannedExceptionData>(ex.ExceptionData);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputLongBannedAsync()
	{
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);

		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);

		Prison prison = UnchainFactory.CreatePrison();
		prison.FailedVerification(coin.Outpoint, round.Id);
		using Arena arena = await ArenaBuilder.From(cfg, rpc, prison).CreateAndStartAsync(round);

		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));

		Assert.Equal(UnchainProtocolErrorCode.InputBanned, ex.ErrorCode);
		Assert.IsAssignableFrom<InputBannedExceptionData>(ex.ExceptionData);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputCantBeNotedAsync()
	{
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);

		UnchainConfig cfg = UnchainFactory.CreateUnchainConfig();
		cfg.AllowNotedInputRegistration = false;

		var round = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		Prison prison = UnchainFactory.CreatePrison();
		prison.FailedToConfirm(coin.Outpoint, Money.Coins(1m), round.Id);

		using Arena arena = await ArenaBuilder.From(cfg, rpc, prison).CreateAndStartAsync(round);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.InputBanned, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputSpentAsync()
	{
		using Key key = new();
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		var mockRpc = new MockRpcClient();
		mockRpc.OnGetTxOutAsync = (_, _, _) => null;

		using Arena arena = await ArenaBuilder.From(cfg).With(mockRpc).CreateAndStartAsync(round);
		var arenaClient = UnchainFactory.CreateArenaClient(arena);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, BitcoinFactory.CreateOutPoint(), ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.InputSpent, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputUnconfirmedAsync()
	{
		using Key key = new();
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		var mockRpc = new MockRpcClient();
		mockRpc.OnGetTxOutAsync = (_, _, _) =>
			new NBitcoin.RPC.GetTxOutResponse { Confirmations = 0 };

		using Arena arena = await ArenaBuilder.From(cfg).With(mockRpc).CreateAndStartAsync(round);
		var arenaClient = UnchainFactory.CreateArenaClient(arena);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, BitcoinFactory.CreateOutPoint(), ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.InputUnconfirmed, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputImmatureAsync()
	{
		using Key key = new();
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		var rpc = UnchainFactory.CreatePreconfiguredRpcClient();
		var callCounter = 1;
		rpc.OnGetTxOutAsync = (_, _, _) =>
		{
			var ret = new NBitcoin.RPC.GetTxOutResponse { Confirmations = callCounter, IsCoinBase = true };
			callCounter++;
			return ret;
		};
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);
		var arenaClient = UnchainFactory.CreateArenaClient(arena);

		var req = UnchainFactory.CreateInputRegistrationRequest(round: round);
		foreach (var i in Enumerable.Range(1, 100))
		{
			var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
				async () => await arenaClient.RegisterInputAsync(round.Id, BitcoinFactory.CreateOutPoint(), ownershipProof, CancellationToken.None));

			Assert.Equal(UnchainProtocolErrorCode.InputImmature, ex.ErrorCode);
		}

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TaprootNotAllowedAsync()
	{
		UnchainConfig cfg = new() { AllowP2trInputs = false };
		var round = UnchainFactory.CreateRound(cfg);

		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key, scriptPubKeyType: ScriptPubKeyType.TaprootBIP86);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var minAliceDeadline = DateTimeOffset.UtcNow + (cfg.ConnectionConfirmationTimeout * 0.9);
		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id, scriptPubKeyType: ScriptPubKeyType.TaprootBIP86);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.ScriptNotAllowed, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task WrongScriptPubKeyInOwnershipProofAsync()
	{
		await TestOwnershipProofAsync((key, roundId) => UnchainFactory.CreateOwnershipProof(new Key(), roundId));
	}

	[Fact]
	public async Task WrongRoundIdInOwnershipProofAsync()
	{
		await TestOwnershipProofAsync((key, roundId) => UnchainFactory.CreateOwnershipProof(key, uint256.One));
	}

	[Fact]
	public async Task WrongCoordinatorIdentifierInOwnershipProofAsync()
	{
		await TestOwnershipProofAsync((key, roundId) => OwnershipProof.GenerateCoinJoinInputProof(key, new OwnershipIdentifier(key, key.PubKey.GetScriptPubKey(ScriptPubKeyType.Segwit)), new CoinJoinInputCommitmentData("test", roundId), ScriptPubKeyType.Segwit));
	}

	[Fact]
	public async Task NotEnoughFundsAsync()
	{
		using Key key = new();
		var txOut = new TxOut(Money.Coins(1.0m), key.GetScriptPubKey(ScriptPubKeyType.Segwit));
		var outpoint = BitcoinFactory.CreateOutPoint();
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(new Coin(outpoint, txOut));

		UnchainConfig cfg = new() { MinRegistrableAmount = Money.Coins(2) };
		var round = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.NotEnoughFunds, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TooMuchFundsAsync()
	{
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		UnchainConfig cfg = new() { MaxRegistrableAmount = Money.Coins(0.9m) };
		var round = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.TooMuchFunds, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TooMuchVsizeAsync()
	{
		// Configures a round that allows so many inputs (Alices) that
		// the virtual size each of they have available is not enough
		// to register anything.
		UnchainConfig cfg = new() { MaxInputCountByRound = 100_000 };
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);

		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		RoundParameterFactory roundParameterFactory = UnchainFactory.CreateRoundParametersFactory(cfg, rpc.Network, maxVsizeAllocationPerAlice: 0);
		Round round = UnchainFactory.CreateRound(roundParameterFactory.CreateRoundParameter(new FeeRate(10m), Money.Zero));
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).With(roundParameterFactory).CreateAndStartAsync(round);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.TooMuchVsize, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task AliceAlreadyRegisteredIntraRoundAsync()
	{
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		// Make sure an Alice have already been registered with the same input.
		var anotherAlice = UnchainFactory.CreateAlice(coin, UnchainFactory.CreateOwnershipProof(key), round);
		round.Alices.Add(anotherAlice);
		round.SetPhase(Phase.ConnectionConfirmation);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.AliceAlreadyRegistered, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task AliceAlreadyRegisteredCrossRoundAsync()
	{
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var anotherRound = UnchainFactory.CreateRound(cfg);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		// Make sure an Alice have already been registered with the same input.
		var preAlice = UnchainFactory.CreateAlice(coin, UnchainFactory.CreateOwnershipProof(key), round);
		anotherRound.Alices.Add(preAlice);
		anotherRound.SetPhase(Phase.ConnectionConfirmation);

		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round, anotherRound);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.AliceAlreadyRegistered, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	private async Task TestOwnershipProofAsync(Func<Key, uint256, OwnershipProof> ownershipProofFunc)
	{
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		OwnershipProof ownershipProof = ownershipProofFunc(key, round.Id);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.WrongOwnershipProof, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}
}
