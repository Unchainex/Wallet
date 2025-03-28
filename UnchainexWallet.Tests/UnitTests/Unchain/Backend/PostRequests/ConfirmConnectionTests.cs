using NBitcoin;
using System.Threading;
using System.Threading.Tasks;
using Unchain.CredentialRequesting;
using Unchain.Crypto;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Models;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PostRequests;

public class ConfirmConnectionTests
{
	[Fact]
	public async Task SuccessInInputRegistrationPhaseAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		var alice = UnchainFactory.CreateAlice(round);
		var preDeadline = alice.Deadline;
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);
		var minAliceDeadline = DateTimeOffset.UtcNow + (cfg.ConnectionConfirmationTimeout * 0.9);

		var resp = await arena.ConfirmConnectionAsync(req, CancellationToken.None);
		Assert.NotNull(resp);
		Assert.NotNull(resp.ZeroAmountCredentials);
		Assert.NotNull(resp.ZeroVsizeCredentials);
		Assert.Null(resp.RealAmountCredentials);
		Assert.Null(resp.RealVsizeCredentials);
		Assert.NotEqual(preDeadline, alice.Deadline);
		Assert.True(minAliceDeadline <= alice.Deadline);
		Assert.False(alice.ConfirmedConnection);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task SuccessInConnectionConfirmationPhaseAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);

		round.SetPhase(Phase.ConnectionConfirmation);
		var alice = UnchainFactory.CreateAlice(round);
		var preDeadline = alice.Deadline;
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);

		var resp = await arena.ConfirmConnectionAsync(req, CancellationToken.None);
		Assert.NotNull(resp);
		Assert.NotNull(resp.ZeroAmountCredentials);
		Assert.NotNull(resp.ZeroVsizeCredentials);
		Assert.NotNull(resp.RealAmountCredentials);
		Assert.NotNull(resp.RealVsizeCredentials);
		Assert.Equal(preDeadline, alice.Deadline);
		Assert.True(alice.ConfirmedConnection);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task RoundNotFoundAsync()
	{
		var cfg = new UnchainConfig();
		var nonExistingRound = UnchainFactory.CreateRound(cfg);
		using Arena arena = await ArenaBuilder.Default.CreateAndStartAsync();
		var req = UnchainFactory.CreateConnectionConfirmationRequest(nonExistingRound);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(
			async () => await arena.ConfirmConnectionAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.RoundNotFound, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task WrongPhaseAsync()
	{
		UnchainConfig cfg = new();
		Round round = UnchainFactory.CreateRound(cfg);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);
		var alice = UnchainFactory.CreateAlice(round);
		var preDeadline = alice.Deadline;
		round.Alices.Add(alice);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);
		foreach (Phase phase in Enum.GetValues(typeof(Phase)))
		{
			if (phase != Phase.InputRegistration && phase != Phase.ConnectionConfirmation)
			{
				round.SetPhase(phase);
				var ex = await Assert.ThrowsAsync<WrongPhaseException>(
					async () => await arena.ConfirmConnectionAsync(req, CancellationToken.None));

				Assert.Equal(UnchainProtocolErrorCode.WrongPhase, ex.ErrorCode);
			}
		}
		Assert.Equal(preDeadline, alice.Deadline);
		Assert.False(alice.ConfirmedConnection);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task AliceNotFoundAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.ConfirmConnectionAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.AliceNotFound, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task IncorrectRequestedVsizeCredentialsAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.ConnectionConfirmation);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);
		Assert.Contains(alice, round.Alices);

		var incorrectVsizeCredentials = UnchainFactory.CreateRealCredentialRequests(round, null, 234).vsizeRequest;
		var req = UnchainFactory.CreateConnectionConfirmationRequest(round) with { RealVsizeCredentialRequests = incorrectVsizeCredentials };

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.ConfirmConnectionAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.IncorrectRequestedVsizeCredentials, ex.ErrorCode);
		Assert.False(alice.ConfirmedConnection);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task IncorrectRequestedAmountCredentialsAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.ConnectionConfirmation);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var incorrectAmountCredentials = UnchainFactory.CreateRealCredentialRequests(round, Money.Coins(3), null).amountRequest;
		var req = UnchainFactory.CreateConnectionConfirmationRequest(round) with { RealAmountCredentialRequests = incorrectAmountCredentials };

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.ConfirmConnectionAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.IncorrectRequestedAmountCredentials, ex.ErrorCode);
		Assert.False(alice.ConfirmedConnection);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task InvalidRequestedAmountCredentialsAsync()
	{
		await InvalidRequestedCredentialsAsync(
			(round) => (round.AmountCredentialIssuer, round.VsizeCredentialIssuer),
			(request) => request.RealAmountCredentialRequests);
	}

	[Fact]
	public async Task InvalidRequestedVsizeCredentialsAsync()
	{
		await InvalidRequestedCredentialsAsync(
			(round) => (round.VsizeCredentialIssuer, round.AmountCredentialIssuer),
			(request) => request.RealVsizeCredentialRequests);
	}

	private async Task InvalidRequestedCredentialsAsync(
		Func<Round, (CredentialIssuer, CredentialIssuer)> credentialIssuerSelector,
		Func<ConnectionConfirmationRequest, RealCredentialsRequest> credentialsRequestSelector)
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.ConnectionConfirmation);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateConnectionConfirmationRequest(round);
		var (issuer, issuer2) = credentialIssuerSelector(round);
		var credentialsRequest = credentialsRequestSelector(req);

		// invalidate serial numbers
		issuer.HandleRequest(credentialsRequest);

		var ex = await Assert.ThrowsAsync<UnchainCryptoException>(async () => await arena.ConfirmConnectionAsync(req, CancellationToken.None));
		Assert.Equal(UnchainCryptoErrorCode.SerialNumberAlreadyUsed, ex.ErrorCode);
		Assert.False(alice.ConfirmedConnection);
		await arena.StopAsync(CancellationToken.None);
	}
}
