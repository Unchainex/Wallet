using NBitcoin;

namespace UnchainexWallet.Blockchain.Transactions.Summary;

public class ForeignOutput : Output
{
	public ForeignOutput(Money amount, BitcoinAddress destinationAddress) : base(amount, destinationAddress)
	{
	}
}
