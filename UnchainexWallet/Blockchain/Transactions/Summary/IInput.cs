using NBitcoin;

namespace UnchainexWallet.Blockchain.Transactions.Summary;

public interface IInput
{
	Money? Amount { get; }
}
