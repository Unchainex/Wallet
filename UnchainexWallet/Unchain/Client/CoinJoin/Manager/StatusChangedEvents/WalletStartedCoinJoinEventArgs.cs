using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client.StatusChangedEvents;

public class WalletStartedCoinJoinEventArgs : StatusChangedEventArgs
{
	public WalletStartedCoinJoinEventArgs(IWallet wallet) : base(wallet)
	{
	}
}
