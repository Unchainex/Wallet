using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Models;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PostRequests;

public class CredentialReissuanceTest
{
	[Fact]
	public async Task ReissueExactDeltaAmountAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		var alice = UnchainFactory.CreateAlice(round);
		round.Alices.Add(alice);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		// Step 1. Create credentials
		var (amClient, vsClient, amIssuer, vsIssuer, amZeroCredentials, vsZeroCredentials) = UnchainFactory.CreateUnchainClientsAndIssuers(round);

		var amountsToRequest = new[] { alice.CalculateRemainingAmountCredentials(round.Parameters.MiningFeeRate).Satoshi };
		var (amCredentialRequest, amValid) = amClient.CreateRequest(
			amountsToRequest,
			amZeroCredentials, // FIXME doesn't make much sense
			CancellationToken.None);
		var startingVsizeCredentialAmount = 100L; // any number is okay here for this test
		var (vsCredentialRequest, weValid) = vsClient.CreateRequest(
			new[] { startingVsizeCredentialAmount },
			vsZeroCredentials, // FIXME doesn't make much sense
			CancellationToken.None);

		var amResp = amIssuer.HandleRequest(amCredentialRequest);
		var weResp = vsIssuer.HandleRequest(vsCredentialRequest);
		var amountCredentialsToPresent = amClient.HandleResponse(amResp, amValid).ToArray();
		var vsizeCredentialsToPresent = vsClient.HandleResponse(weResp, weValid).ToArray();

		// Step 2.
		var invalidVsizesToRequest = vsizeCredentialsToPresent.Select(x => 2 * x.Value); // we request the double than what we have.
		var (realVsizeCredentialRequest, realVsizeCredentialResponseValidation) = vsClient.CreateRequest(
			invalidVsizesToRequest,
			vsizeCredentialsToPresent,
			CancellationToken.None);

		var (realAmountCredentialRequest, realAmountCredentialResponseValidation) = amClient.CreateRequest(
			amountsToRequest,
			amountCredentialsToPresent,
			CancellationToken.None);

		var zeroAmountCredentialRequestData = amClient.CreateRequestForZeroAmount();
		var zeroVsizeCredentialRequestData = vsClient.CreateRequestForZeroAmount();

		// hit Arena directly to verify it prevents requesting more vsize credentials than what are presented.
		// we have to bypass the ArenaClient because it also prevents this invalid requests and breaks the circuit
		// early, not allowing to hit Arena.
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () =>
			await arena.ReissuanceAsync(
				new ReissueCredentialRequest(
					round.Id,
					realAmountCredentialRequest,
					realVsizeCredentialRequest,
					zeroAmountCredentialRequestData.CredentialsRequest,
					zeroVsizeCredentialRequestData.CredentialsRequest),
				CancellationToken.None).ConfigureAwait(false));
		Assert.Equal(UnchainProtocolErrorCode.DeltaNotZero, ex.ErrorCode);

		// Step 2a. Now we verify the client also implements the same verifications.
		var arenaClient = UnchainFactory.CreateArenaClient(arena);
		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await arenaClient.ReissueCredentialAsync(
				round.Id,
				amountsToRequest,
				invalidVsizesToRequest, // we request the double than what we can
				amountCredentialsToPresent,
				vsizeCredentialsToPresent,
				CancellationToken.None));

		await arena.StopAsync(CancellationToken.None);
	}
}
