using NBitcoin;

namespace UnchainexWallet.Exceptions;

public class NotSupportedNetworkException : NotSupportedException
{
	public NotSupportedNetworkException(Network? network)
		: base(network is null ? $"{nameof(Network)} wasn't specified." : $"{nameof(Network)} not supported: {network}.")
	{
	}
}
