using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client.StatusChangedEvents;

public class CompletedEventArgs : StatusChangedEventArgs
{
	public CompletedEventArgs(IWallet wallet, CompletionStatus completionStatus)
		: base(wallet)
	{
		CompletionStatus = completionStatus;
	}

	public CompletionStatus CompletionStatus { get; }
}
