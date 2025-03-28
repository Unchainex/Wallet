using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client.StatusChangedEvents;

public class LoadedEventArgs : StatusChangedEventArgs
{
	public LoadedEventArgs(IWallet wallet)
		: base(wallet)
	{
	}
}
