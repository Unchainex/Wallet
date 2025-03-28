using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Client;
using UnchainexWallet.Unchain.Client.CoinJoin.Client;
using UnchainexWallet.Unchain.Client.RoundStateAwaiters;
using UnchainexWallet.Unchain.Models;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend;

public class AliceTimeoutTests
{
	[Fact]
	public async Task AliceRegistrationTimesOutAsync()
	{
		using CancellationTokenSource testDeadlineCts = new(TimeSpan.FromMinutes(5)); // Sanity timeout for the unit test.

		// Alice times out when its deadline is reached.
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var km = ServiceFactory.CreateKeyManager(password: "");
		var key = BitcoinFactory.CreateHdPubKey(km);
		var smartCoin = BitcoinFactory.CreateSmartCoin(key, 10m);
		var rpc = UnchainFactory.CreatePreconfiguredRpcClient(smartCoin.Coin);

		using Arena arena = await ArenaBuilder.From(cfg).With(rpc).CreateAndStartAsync(round);
		var arenaClient = UnchainFactory.CreateArenaClient(arena);

		using RoundStateUpdater roundStateUpdater = new(TimeSpan.FromSeconds(2), arena);
		await roundStateUpdater.StartAsync(testDeadlineCts.Token);

		// Register Alices.
		KeyChain keyChain = new(km, "");

		using CancellationTokenSource registrationCts = new();
		Task<AliceClient> task = AliceClient.CreateRegisterAndConfirmInputAsync(RoundState.FromRound(round), arenaClient, smartCoin, keyChain, roundStateUpdater, registrationCts.Token, registrationCts.Token, confirmationCancellationToken: testDeadlineCts.Token);

		while (round.Alices.Count == 0)
		{
			await Task.Delay(10, testDeadlineCts.Token);
		}

		var alice = Assert.Single(round.Alices);
		alice.Deadline = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));

		Assert.Empty(round.Alices);

		registrationCts.Cancel();

		try
		{
			await task;
			throw new InvalidOperationException("The operation should throw!");
		}
		catch (Exception ex)
		{
			Assert.True(ex is OperationCanceledException or UnchainProtocolException);
		}

		await roundStateUpdater.StopAsync(testDeadlineCts.Token);
		await arena.StopAsync(testDeadlineCts.Token);
	}

	[Fact]
	public async Task AliceDoesntTimeoutInConnectionConfirmationAsync()
	{
		// Alice does not time out when it's not input registration anymore,
		// even though the deadline is reached.
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.ConnectionConfirmation);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);

		Assert.Single(round.Alices);
		DateTimeOffset preDeadline = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
		alice.Deadline = preDeadline;
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Single(round.Alices);
		Assert.Equal(preDeadline, alice.Deadline);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task AliceDoesntTimeoutIfInputRegistrationTimedoutAsync()
	{
		// Alice does not time out if input registration timed out,
		// even though the deadline is reached and still in input reg.
		UnchainConfig cfg = new() { StandardInputRegistrationTimeout = TimeSpan.Zero };
		var round = UnchainFactory.CreateRound(cfg);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);

		Assert.Single(round.Alices);
		DateTimeOffset preDeadline = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
		alice.Deadline = preDeadline;
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Single(round.Alices);
		Assert.Equal(preDeadline, alice.Deadline);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task AliceDoesntTimeoutIfMaxInputCountReachedAsync()
	{
		// Alice does not time out if input reg is full with alices,
		// even though the deadline is reached and still in input reg.
		UnchainConfig cfg = new() { MaxInputCountByRound = 3 };
		var round = UnchainFactory.CreateRound(cfg);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);

		Assert.Equal(3, round.Alices.Count);
		DateTimeOffset preDeadline = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
		alice.Deadline = preDeadline;
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Equal(3, round.Alices.Count);
		Assert.Equal(preDeadline, alice.Deadline);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task AliceDeadlineUpdatedAsync()
	{
		// Alice's deadline is updated by connection confirmation.
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);

		Assert.Single(round.Alices);
		DateTimeOffset preDeadline = DateTimeOffset.UtcNow - TimeSpan.FromMilliseconds(1);
		alice.Deadline = preDeadline;
		await arena.ConfirmConnectionAsync(req, CancellationToken.None);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Single(round.Alices);
		Assert.NotEqual(preDeadline, alice.Deadline);

		await arena.StopAsync(CancellationToken.None);
	}
}
