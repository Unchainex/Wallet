using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client.StatusChangedEvents;

public class StartedEventArgs : StatusChangedEventArgs
{
	public StartedEventArgs(IWallet wallet, TimeSpan registrationTimeout)
		: base(wallet)
	{
		RegistrationTimeout = registrationTimeout;
	}

	public TimeSpan RegistrationTimeout { get; }
}
