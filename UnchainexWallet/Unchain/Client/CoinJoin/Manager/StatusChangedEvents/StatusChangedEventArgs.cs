using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client.StatusChangedEvents;

public enum StopReason
{
	WalletUnloaded
}

public enum CompletionStatus
{
	Success,
	Canceled,
	Failed,
	Unknown
}

public enum CoinjoinError
{
	NoCoinsEligibleToMix,
	AutoConjoinDisabled,
	UserInSendWorkflow,
	NotEnoughUnprivateBalance,
	BackendNotSynchronized,
	AllCoinsPrivate,
	UserWasntInRound,
	NoConfirmedCoinsEligibleToMix,
	CoinsRejected,
	OnlyImmatureCoinsAvailable,
	OnlyExcludedCoinsAvailable,
	UneconomicalRound,
	MiningFeeRateTooHigh,
	MinInputCountTooLow,
	CoordinatorLiedAboutInputs,
	NotEnoughConfirmedUnprivateBalance
}

public class StatusChangedEventArgs : EventArgs
{
	public StatusChangedEventArgs(IWallet wallet)
	{
		Wallet = wallet;
	}

	public IWallet Wallet { get; }
}
