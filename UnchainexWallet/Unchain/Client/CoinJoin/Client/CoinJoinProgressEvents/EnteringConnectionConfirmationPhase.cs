using UnchainexWallet.Unchain.Models;

namespace UnchainexWallet.Unchain.Client.CoinJoinProgressEvents;

public class EnteringConnectionConfirmationPhase : RoundStateChanged
{
	public EnteringConnectionConfirmationPhase(RoundState roundState, DateTimeOffset timeoutAt) : base(roundState, timeoutAt)
	{
	}
}
