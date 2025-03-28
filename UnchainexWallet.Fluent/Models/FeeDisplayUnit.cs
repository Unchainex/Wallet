using UnchainexWallet.Models;

namespace UnchainexWallet.Fluent.Models;

public enum FeeDisplayUnit
{
	BTC,

	[FriendlyName("sats")]
	Satoshis,
}
