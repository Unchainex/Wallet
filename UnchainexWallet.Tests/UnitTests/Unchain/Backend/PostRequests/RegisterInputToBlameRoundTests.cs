using NBitcoin;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.DoSPrevention;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PostRequests;

public class RegisterInputToBlameRoundTests
{
	[Fact]
	public async Task InputNotWhitelistedAsync()
	{
		UnchainConfig cfg = new();
		using Key key = new();
		var coin = UnchainFactory.CreateCoin(key);
		var mockRpc = UnchainFactory.CreatePreconfiguredRpcClient(coin);

		var round = UnchainFactory.CreateRound(cfg);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		Round blameRound = UnchainFactory.CreateBlameRound(round, cfg);
		using Arena arena = await ArenaBuilder.From(cfg).With(mockRpc).CreateAndStartAsync(round, blameRound);

		var req = UnchainFactory.CreateInputRegistrationRequest(round: blameRound, key, coin.Outpoint);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterInputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.InputNotWhitelisted, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputWhitelistedAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		Round blameRound = UnchainFactory.CreateBlameRound(round, cfg);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round, blameRound);

		var req = UnchainFactory.CreateInputRegistrationRequest(prevout: alice.Coin.Outpoint, round: blameRound);

		var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await arena.RegisterInputAsync(req, CancellationToken.None));
		if (ex is UnchainProtocolException wspex)
		{
			Assert.NotEqual(UnchainProtocolErrorCode.InputNotWhitelisted, wspex.ErrorCode);
		}

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InputWhitelistedButBannedAsync()
	{
		UnchainConfig cfg = UnchainFactory.CreateUnchainConfig();
		var round = UnchainFactory.CreateRound(cfg);

		using Key key = new();
		var alice = UnchainFactory.CreateAlice(key, round);
		var bannedCoin = alice.Coin.Outpoint;

		round.Alices.Add(alice);
		Round blameRound = UnchainFactory.CreateBlameRound(round, cfg);
		var mockRpc = UnchainFactory.CreatePreconfiguredRpcClient(alice.Coin);

		Prison prison = UnchainFactory.CreatePrison();
		using Arena arena = await ArenaBuilder.From(cfg, mockRpc, prison).CreateAndStartAsync(round, blameRound);

		prison.FailedToConfirm(bannedCoin, alice.Coin.Amount, round.Id);

		var req = UnchainFactory.CreateInputRegistrationRequest(key: key, round: blameRound, prevout: bannedCoin);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterInputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.InputBanned, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}
}
