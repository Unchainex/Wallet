using NBitcoin;
using UnchainexWallet.Blockchain.TransactionBuilding;

namespace UnchainexWallet.Rpc;

public class PaymentInfo
{
	public required Destination Sendto { get; init; }
	public required Money Amount { get; init; }
	public required string Label { get; init; }
	public bool SubtractFee { get; init; }
}
