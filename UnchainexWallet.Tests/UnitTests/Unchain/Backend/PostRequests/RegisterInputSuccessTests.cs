using NBitcoin;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnchainexWallet.Crypto;
using UnchainexWallet.Crypto.Randomness;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Models;
using Xunit;
using UnchainexWallet.Unchain.Client.CoinJoin.Client;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PostRequests;

public class RegisterInputSuccessTests
{
	private static void AssertSingleAliceSuccessfullyRegistered(Round round, DateTimeOffset minAliceDeadline, ArenaResponse<Guid> resp)
	{
		var alice = Assert.Single(round.Alices);
		Assert.NotNull(resp);
		Assert.NotNull(resp.IssuedAmountCredentials);
		Assert.NotNull(resp.IssuedVsizeCredentials);
		Assert.True(minAliceDeadline <= alice.Deadline);
	}

	[Fact]
	public async Task SuccessAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);

		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var minAliceDeadline = DateTimeOffset.UtcNow + cfg.ConnectionConfirmationTimeout * 0.9;
		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);

		var resp = await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None);
		AssertSingleAliceSuccessfullyRegistered(round, minAliceDeadline, resp);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task SuccessCustomCoordinatorIdentifierAsync()
	{
		UnchainConfig cfg = new();
		cfg.CoordinatorIdentifier = "test";
		var round = UnchainFactory.CreateRound(cfg);

		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var minAliceDeadline = DateTimeOffset.UtcNow + cfg.ConnectionConfirmationTimeout * 0.9;

		var roundState = RoundState.FromRound(arena.Rounds.First());
		var arenaClient = new ArenaClient(
			roundState.CreateAmountCredentialClient(InsecureRandom.Instance),
			roundState.CreateVsizeCredentialClient(InsecureRandom.Instance),
			"test",
			arena);
		var ownershipProof = OwnershipProof.GenerateCoinJoinInputProof(key, new OwnershipIdentifier(key, key.PubKey.GetScriptPubKey(ScriptPubKeyType.Segwit)), new CoinJoinInputCommitmentData("test", round.Id), ScriptPubKeyType.Segwit);

		var resp = await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None);
		AssertSingleAliceSuccessfullyRegistered(round, minAliceDeadline, resp);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task SuccessWithAliceUpdateIntraRoundAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);

		using Key key = new();
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id);
		var coin = UnchainFactory.CreateCoin(key);

		// Make sure an Alice have already been registered with the same input.
		var preAlice = UnchainFactory.CreateAlice(coin, UnchainFactory.CreateOwnershipProof(key), round);
		round.Alices.Add(preAlice);

		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None).ConfigureAwait(false));
		Assert.Equal(UnchainProtocolErrorCode.AliceAlreadyRegistered, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TaprootSuccessAsync()
	{
		UnchainConfig cfg = new() { AllowP2trInputs = true };
		var round = UnchainFactory.CreateRound(cfg);

		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key, scriptPubKeyType: ScriptPubKeyType.TaprootBIP86);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);
		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);

		var minAliceDeadline = DateTimeOffset.UtcNow + cfg.ConnectionConfirmationTimeout * 0.9;
		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		var ownershipProof = UnchainFactory.CreateOwnershipProof(key, round.Id, ScriptPubKeyType.TaprootBIP86);

		var resp = await arenaClient.RegisterInputAsync(round.Id, coin.Outpoint, ownershipProof, CancellationToken.None);
		AssertSingleAliceSuccessfullyRegistered(round, minAliceDeadline, resp);

		await arena.StopAsync(CancellationToken.None);
	}
}
