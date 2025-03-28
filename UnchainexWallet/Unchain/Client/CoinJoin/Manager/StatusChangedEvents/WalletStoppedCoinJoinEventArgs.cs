using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client.StatusChangedEvents;

public class WalletStoppedCoinJoinEventArgs : StatusChangedEventArgs
{
	public WalletStoppedCoinJoinEventArgs(IWallet wallet) : base(wallet)
	{
	}
}
