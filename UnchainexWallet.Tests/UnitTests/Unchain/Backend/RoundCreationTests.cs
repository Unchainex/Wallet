using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.BitcoinRpc;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Rounds;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend;

public class RoundCreationTests
{
	private static Arena CreateArena(UnchainConfig cfg, IRPCClient rpc)
	{
		var arenaBuilder = ArenaBuilder.From(cfg).With(rpc);
		arenaBuilder.Period = TimeSpan.FromSeconds(1);
		return arenaBuilder.Create();
	}

	[Fact]
	public async Task InitializesRoundAsync()
	{
		UnchainConfig cfg = new();
		var mockRpc = BitcoinFactory.GetMockMinimalRpc();

		using Arena arena = CreateArena(cfg, mockRpc);
		Assert.Empty(arena.Rounds);
		await arena.StartAsync(CancellationToken.None);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Single(arena.Rounds);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task CreatesRoundIfNoneInputRegistrationAsync()
	{
		UnchainConfig cfg = new();
		var mockRpc = BitcoinFactory.GetMockMinimalRpc();

		using Arena arena = CreateArena(cfg, mockRpc);
		Assert.Empty(arena.Rounds);
		await arena.StartAsync(CancellationToken.None);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		var round = Assert.Single(arena.Rounds);

		round.SetPhase(Phase.ConnectionConfirmation);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Equal(2, arena.Rounds.Count);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task CreatesRoundIfInBlameInputRegistrationAsync()
	{
		UnchainConfig cfg = new();
		var mockRpc = BitcoinFactory.GetMockMinimalRpc();

		using Arena arena = CreateArena(cfg, mockRpc);
		Assert.Empty(arena.Rounds);
		await arena.StartAsync(CancellationToken.None);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		var round = Assert.Single(arena.Rounds);

		round.SetPhase(Phase.ConnectionConfirmation);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		Round blameRound = UnchainFactory.CreateBlameRound(round, cfg);
		Assert.Equal(Phase.InputRegistration, blameRound.Phase);
		arena.Rounds.Add(blameRound);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		Assert.Equal(3, arena.Rounds.Count);
		Assert.Equal(2, arena.Rounds.Where(x => x.Phase == Phase.InputRegistration).Count());

		await arena.StopAsync(CancellationToken.None);
	}
}
