using System.Collections.Immutable;
using NBitcoin;

namespace UnchainexWallet.Unchain.Models;
public record RoundStateCheckpoint(uint256 RoundId, int StateId);

public record RoundStateRequest(ImmutableList<RoundStateCheckpoint> RoundCheckpoints)
{
	public static readonly RoundStateRequest Empty = new(ImmutableList<RoundStateCheckpoint>.Empty);
}
