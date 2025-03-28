using NBitcoin;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Models;
using UnchainexWallet.Unchain.Models.MultipartyTransaction;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PostRequests;

public class SignTransactionTests
{
	[Fact]
	public async Task SuccessAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		using Key key = new();
		Alice alice = UnchainFactory.CreateAlice(key: key, round: round);
		round.Alices.Add(alice);
		round.CoinjoinState = round.AddInput(alice.Coin, alice.OwnershipProof, UnchainFactory.CreateCommitmentData(round.Id)).Finalize();
		round.SetPhase(Phase.TransactionSigning);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var aliceSignedCoinJoin = round.Assert<SigningState>().CreateUnsignedTransaction();
		aliceSignedCoinJoin.Sign(key.GetBitcoinSecret(Network.Main), alice.Coin);

		var req = new TransactionSignaturesRequest(round.Id, 0, aliceSignedCoinJoin.Inputs[0].WitScript);
		await arena.SignTransactionAsync(req, CancellationToken.None);
		Assert.True(round.Assert<SigningState>().IsFullySigned);
		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TaprootSuccessAsync()
	{
		UnchainConfig cfg = new() { AllowP2trInputs = true, AllowP2trOutputs = true };
		var round = UnchainFactory.CreateRound(cfg);
		using Key key = new();
		Alice alice = UnchainFactory.CreateAlice(key: key, round: round, scriptPubKeyType: ScriptPubKeyType.TaprootBIP86);
		round.Alices.Add(alice);
		round.CoinjoinState = round.AddInput(alice.Coin, alice.OwnershipProof, UnchainFactory.CreateCommitmentData(round.Id)).Finalize();
		round.SetPhase(Phase.TransactionSigning);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var aliceSignedCoinJoin = round.Assert<SigningState>().CreateUnsignedTransaction();
		aliceSignedCoinJoin.Sign(key.GetBitcoinSecret(Network.Main), alice.Coin);

		var req = new TransactionSignaturesRequest(round.Id, 0, aliceSignedCoinJoin.Inputs[0].WitScript);
		await arena.SignTransactionAsync(req, CancellationToken.None);
		Assert.True(round.Assert<SigningState>().IsFullySigned);
		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task RoundNotFoundAsync()
	{
		using Arena arena = await ArenaBuilder.Default.CreateAndStartAsync();
		var req = new TransactionSignaturesRequest(uint256.Zero, 0, WitScript.Empty);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.SignTransactionAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.RoundNotFound, ex.ErrorCode);
		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task WrongPhaseAsync()
	{
		UnchainConfig cfg = new();
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync();
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));
		var round = arena.Rounds.First();

		var req = new TransactionSignaturesRequest(round.Id, 0, WitScript.Empty);
		foreach (Phase phase in Enum.GetValues(typeof(Phase)))
		{
			if (phase != Phase.TransactionSigning)
			{
				round.SetPhase(phase);

				var ex = await Assert.ThrowsAsync<WrongPhaseException>(async () =>
					await arena.SignTransactionAsync(req, CancellationToken.None));
				Assert.Equal(UnchainProtocolErrorCode.WrongPhase, ex.ErrorCode);
			}
		}

		await arena.StopAsync(CancellationToken.None);
	}
}
