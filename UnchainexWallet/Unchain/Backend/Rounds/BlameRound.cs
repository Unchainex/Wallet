using NBitcoin;
using System.Collections.Generic;
using Unchain.Crypto.Randomness;

namespace UnchainexWallet.Unchain.Backend.Rounds;

public class BlameRound : Round
{
	public BlameRound(RoundParameters parameters, Round blameOf, ISet<OutPoint> blameWhitelist, UnchainexRandom random)
		: base(parameters, random)
	{
		BlameOf = blameOf;
		BlameWhitelist = blameWhitelist;
		InputRegistrationTimeFrame = TimeFrame.Create(Parameters.BlameInputRegistrationTimeout).StartNow();
	}

	public Round BlameOf { get; }
	public ISet<OutPoint> BlameWhitelist { get; }

	public override bool IsInputRegistrationEnded(int maxInputCount)
	{
		return base.IsInputRegistrationEnded(BlameWhitelist.Count);
	}
}
