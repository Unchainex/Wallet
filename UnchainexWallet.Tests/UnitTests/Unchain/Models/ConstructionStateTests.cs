using NBitcoin;
using UnchainexWallet.Helpers;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Models.MultipartyTransaction;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Models;

public class ConstructionStateTests
{
	[Fact]
	public void ConstructionStateFeeRateCalculation()
	{
		var miningFeeRate = new FeeRate(8m);
		var cfg = new UnchainConfig();
		var roundParameters = RoundParameters.Create(
				cfg,
				Network.Main,
				miningFeeRate,
				Money.Coins(10));

		var round = UnchainFactory.CreateRound(roundParameters);
		var state = round.Assert<ConstructionState>();

		var (coin, ownershipProof) = UnchainFactory.CreateCoinWithOwnershipProof(
			amount: roundParameters.AllowedInputAmounts.Min + miningFeeRate.GetFee(Constants.P2wpkhInputVirtualSize + Constants.P2wpkhOutputVirtualSize),
			roundId: round.Id);
		state = state.AddInput(coin, ownershipProof, UnchainFactory.CreateCommitmentData(round.Id));
		state = state.AddOutput(new TxOut(roundParameters.AllowedInputAmounts.Min, new Script("0 bf3593d140d512eb607b3ddb5c5ee085f1e3a210")));

		var signingState = state.Finalize();
		Assert.Equal(miningFeeRate, signingState.EffectiveFeeRate);
	}
}
