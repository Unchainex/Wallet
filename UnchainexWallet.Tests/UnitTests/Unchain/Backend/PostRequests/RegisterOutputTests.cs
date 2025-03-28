using NBitcoin;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Extensions;
using UnchainexWallet.Helpers;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Models.MultipartyTransaction;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.PostRequests;

public class RegisterOutputTests
{
	[Fact]
	public async Task SuccessAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateOutputRegistrationRequest(round);
		await arena.RegisterOutputAsync(req, CancellationToken.None);
		Assert.NotEmpty(round.Bobs);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task LegacyOutputsSuccessAsync()
	{
		UnchainConfig cfg = new() { AllowP2pkhOutputs = true, AllowP2shOutputs = true };
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		// p2pkh
		using Key privKey0 = new();
		var pkhScript = privKey0.PubKey.GetScriptPubKey(ScriptPubKeyType.Legacy);
		var req0 = UnchainFactory.CreateOutputRegistrationRequest(round, pkhScript, pkhScript.EstimateOutputVsize());
		await arena.RegisterOutputAsync(req0, CancellationToken.None);
		Assert.Single(round.Bobs);

		// p2sh
		using Key privKey1 = new();
		var shScript = privKey1.PubKey.ScriptPubKey.Hash.ScriptPubKey;
		var req1 = UnchainFactory.CreateOutputRegistrationRequest(round, shScript, shScript.EstimateOutputVsize());
		await arena.RegisterOutputAsync(req1, CancellationToken.None);
		Assert.Equal(2, round.Bobs.Count);
		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TaprootSuccessAsync()
	{
		UnchainConfig cfg = new() { AllowP2trOutputs = true };
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		using Key privKey = new();
		var req = UnchainFactory.CreateOutputRegistrationRequest(round, privKey.PubKey.GetScriptPubKey(ScriptPubKeyType.TaprootBIP86), Constants.P2trOutputVirtualSize);
		await arena.RegisterOutputAsync(req, CancellationToken.None);
		Assert.NotEmpty(round.Bobs);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TaprootNotAllowedAsync()
	{
		UnchainConfig cfg = new() { AllowP2trOutputs = false };
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		using Key privKey = new();
		var req = UnchainFactory.CreateOutputRegistrationRequest(round, privKey.PubKey.GetScriptPubKey(ScriptPubKeyType.TaprootBIP86), Constants.P2trOutputVirtualSize);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.ScriptNotAllowed, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task RoundNotFoundAsync()
	{
		var cfg = new UnchainConfig();
		var nonExistingRound = UnchainFactory.CreateRound(cfg);
		using Arena arena = await ArenaBuilder.Default.CreateAndStartAsync();
		var req = UnchainFactory.CreateOutputRegistrationRequest(nonExistingRound);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.RoundNotFound, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task ScriptNotAllowedAsync()
	{
		using Key key = new();
		var outputScript = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ScriptPubKey;

		UnchainConfig cfg = new();
		RoundParameters parameters = UnchainFactory.CreateRoundParameters(cfg)
			with
		{ MaxVsizeAllocationPerAlice = Constants.P2wpkhInputVirtualSize + outputScript.EstimateOutputVsize() };
		var round = UnchainFactory.CreateRound(parameters);

		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(Money.Coins(1), round));

		var req = UnchainFactory.CreateOutputRegistrationRequest(round, outputScript);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.ScriptNotAllowed, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task NonStandardOutputAsync()
	{
		var sha256Bounty = Script.FromHex("aa20000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f87");
		UnchainConfig cfg = new();
		RoundParameters parameters = UnchainFactory.CreateRoundParameters(cfg)
			with
		{ MaxVsizeAllocationPerAlice =  Constants.P2wpkhInputVirtualSize + sha256Bounty.EstimateOutputVsize() };
		var round = UnchainFactory.CreateRound(parameters);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(Money.Coins(1), round));

		var req = UnchainFactory.CreateOutputRegistrationRequest(round, sha256Bounty);
		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));

		// The following assertion requires standardness to be checked before allowed script types
		Assert.Equal(UnchainProtocolErrorCode.NonStandardOutput, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task NotEnoughFundsAsync()
	{
		UnchainConfig cfg = new() { MinRegistrableAmount = Money.Coins(2) };
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(Money.Coins(1), round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateOutputRegistrationRequest(round);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.NotEnoughFunds, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task TooMuchFundsAsync()
	{
		UnchainConfig cfg = new() { MaxRegistrableAmount = Money.Coins(1.993m) }; // TODO migrate to MultipartyTransactionParameters
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(Money.Coins(2), round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateOutputRegistrationRequest(round);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.TooMuchFunds, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task IncorrectRequestedVsizeCredentialsAsync()
	{
		UnchainConfig cfg = new();
		var round = UnchainFactory.CreateRound(cfg);
		round.SetPhase(Phase.OutputRegistration);
		round.Alices.Add(UnchainFactory.CreateAlice(round));
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);

		var req = UnchainFactory.CreateOutputRegistrationRequest(round, vsize: 30);

		var ex = await Assert.ThrowsAsync<UnchainProtocolException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));
		Assert.Equal(UnchainProtocolErrorCode.IncorrectRequestedVsizeCredentials, ex.ErrorCode);

		await arena.StopAsync(CancellationToken.None);
	}

	[Fact]
	public async Task WrongPhaseAsync()
	{
		UnchainConfig cfg = new();
		Round round = UnchainFactory.CreateRound(cfg);
		using Arena arena = await ArenaBuilder.From(cfg).CreateAndStartAsync(round);
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));

		// Refresh the Arena States because of vsize manipulation.
		await arena.TriggerAndWaitRoundAsync(TimeSpan.FromSeconds(21));

		round.Alices.Add(UnchainFactory.CreateAlice(round));

		foreach (Phase phase in Enum.GetValues(typeof(Phase)))
		{
			if (phase != Phase.OutputRegistration)
			{
				var req = UnchainFactory.CreateOutputRegistrationRequest(round);
				round.SetPhase(phase);
				var ex = await Assert.ThrowsAsync<WrongPhaseException>(async () => await arena.RegisterOutputAsync(req, CancellationToken.None));
				Assert.Equal(UnchainProtocolErrorCode.WrongPhase, ex.ErrorCode);
			}
		}

		await arena.StopAsync(CancellationToken.None);
	}
}
