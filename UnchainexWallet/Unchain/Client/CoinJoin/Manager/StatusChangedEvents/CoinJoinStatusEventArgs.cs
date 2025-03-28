using UnchainexWallet.Unchain.Client.CoinJoinProgressEvents;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client.StatusChangedEvents;

public class CoinJoinStatusEventArgs : StatusChangedEventArgs
{
	public CoinJoinStatusEventArgs(IWallet wallet, CoinJoinProgressEventArgs coinJoinProgressEventArgs) : base(wallet)
	{
		CoinJoinProgressEventArgs = coinJoinProgressEventArgs;
	}

	public CoinJoinProgressEventArgs CoinJoinProgressEventArgs { get; }
}
