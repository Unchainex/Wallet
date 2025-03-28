using NBitcoin;

namespace UnchainexWallet.Blockchain.Transactions.Summary;

public class ForeignInput : IInput
{
	public Money? Amount => default;
}
