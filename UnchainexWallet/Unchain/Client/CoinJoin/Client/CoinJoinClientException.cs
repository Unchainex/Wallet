using UnchainexWallet.Unchain.Client.StatusChangedEvents;

namespace UnchainexWallet.Unchain.Client.CoinJoin.Client;

public class CoinJoinClientException : Exception
{
	public CoinJoinClientException(CoinjoinError coinjoinError, string? message = null) : base($"Coinjoin aborted with error: {coinjoinError}. {message}")
	{
		CoinjoinError = coinjoinError;
	}

	public CoinjoinError CoinjoinError { get; }
}
