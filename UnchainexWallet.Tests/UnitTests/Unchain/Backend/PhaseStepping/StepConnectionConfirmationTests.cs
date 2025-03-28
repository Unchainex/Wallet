using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Tests.UnitTests.Unchain.Backend.Rounds.Utils;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.DoSPrevention;
using UnchainexWallet.Unchain.Backend.Rounds;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PhaseStepping;

public class StepConnectionConfirmationTests
{
	[Fact]
	public async Task AllConfirmedStepsAsync()
	{
		UnchainConfig cfg = new() { MaxInputCountByRound = 4, MinInputCountByRoundMultiplier = 0.5 };
		var round = UnchainFactory.CreateRound(cfg);
		var a1 = UnchainFactory.CreateAlice(round);
		var a2 = UnchainFactory.CreateAlice(round);
		var a3 = UnchainFactory.CreateAlice(round);
		var a4 = UnchainFactory.CreateAlice(round);
		a1.ConfirmedConnection = true;
		a2.ConfirmedConnection = true;
		a3.ConfirmedConnection = true;
		a4.ConfirmedConnection = true;
		round.Alices.Add(a1);
		round.Alices.Add(a2);
		round.Alices.Add(a3);
		round.Alices.Add(a4);
		round.SetPhase(Phase.ConnectionConfirmation);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Equal(Phase.OutputRegistration, round.Phase);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task NotAllConfirmedStaysAsync()
	{
		UnchainConfig cfg = new() { MaxInputCountByRound = 4, MinInputCountByRoundMultiplier = 0.5 };
		var round = UnchainFactory.CreateRound(cfg);
		var a1 = UnchainFactory.CreateAlice(round);
		var a2 = UnchainFactory.CreateAlice(round);
		var a3 = UnchainFactory.CreateAlice(round);
		var a4 = UnchainFactory.CreateAlice(round);
		a1.ConfirmedConnection = true;
		a2.ConfirmedConnection = true;
		a3.ConfirmedConnection = true;
		a4.ConfirmedConnection = false;
		round.Alices.Add(a1);
		round.Alices.Add(a2);
		round.Alices.Add(a3);
		round.Alices.Add(a4);
		round.SetPhase(Phase.ConnectionConfirmation);

		Prison prison = UnchainFactory.CreatePrison();
		using Arena arena = await ArenaBuilder.From(cfg, prison).CreateAndStartAsync(round);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Equal(Phase.ConnectionConfirmation, round.Phase);
		Assert.All(round.Alices, a => Assert.False(prison.IsBanned(a.Coin.Outpoint, cfg.GetDoSConfiguration(), DateTimeOffset.UtcNow)));

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task EnoughConfirmedTimedoutStepsAsync()
	{
		UnchainConfig cfg = UnchainFactory.CreateUnchainConfig();
		cfg.MaxInputCountByRound = 4;
		cfg.ConnectionConfirmationTimeout = TimeSpan.Zero;

		var round = UnchainFactory.CreateRound(cfg);
		var a1 = UnchainFactory.CreateAlice(round);
		var a2 = UnchainFactory.CreateAlice(round);
		var a3 = UnchainFactory.CreateAlice(round);
		var a4 = UnchainFactory.CreateAlice(round);
		a1.ConfirmedConnection = true;
		a2.ConfirmedConnection = true;
		a3.ConfirmedConnection = false;
		a4.ConfirmedConnection = false;
		round.Alices.Add(a1);
		round.Alices.Add(a2);
		round.Alices.Add(a3);
		round.Alices.Add(a4);
		round.SetPhase(Phase.ConnectionConfirmation);

		Prison prison = UnchainFactory.CreatePrison();
		using Arena arena = await ArenaBuilder.From(cfg, prison).CreateAndStartAsync(round);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));

		Assert.Equal(Phase.OutputRegistration, round.Phase);
		Assert.Equal(2, round.Alices.Count);
		var offendingAlices = new[] { a3, a4 };
		Assert.All(offendingAlices, alice => Assert.True(prison.IsBanned(alice.Coin.Outpoint, cfg.GetDoSConfiguration(), DateTimeOffset.UtcNow)));

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task NotEnoughConfirmedTimedoutDestroysAsync()
	{
		UnchainConfig cfg = UnchainFactory.CreateUnchainConfig();
		cfg.MaxInputCountByRound = 4;
		cfg.ConnectionConfirmationTimeout = TimeSpan.Zero;
		cfg.MinInputCountByRoundMultiplier = 0.9;

		var round = UnchainFactory.CreateRound(cfg);
		var a1 = UnchainFactory.CreateAlice(round);
		var a2 = UnchainFactory.CreateAlice(round);
		var a3 = UnchainFactory.CreateAlice(round);
		var a4 = UnchainFactory.CreateAlice(round);
		a1.ConfirmedConnection = true;
		a2.ConfirmedConnection = false;
		a3.ConfirmedConnection = false;
		a4.ConfirmedConnection = false;
		round.Alices.Add(a1);
		round.Alices.Add(a2);
		round.Alices.Add(a3);
		round.Alices.Add(a4);
		round.SetPhase(Phase.ConnectionConfirmation);

		Prison prison = UnchainFactory.CreatePrison();
		using Arena arena = await ArenaBuilder.From(cfg, prison).CreateAndStartAsync(round);

		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.DoesNotContain(round, arena.GetActiveRounds());
		var offendingAlices = new[] { a2, a3, a4 };
		Assert.All(offendingAlices, alice => Assert.True(prison.IsBanned(alice.Coin.Outpoint, cfg.GetDoSConfiguration(), DateTimeOffset.UtcNow)));

		await arena.StopAsync(CancellationToken.None);
	}
}
