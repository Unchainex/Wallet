using NBitcoin;
using UnchainexWallet.Blockchain.TransactionBuilding;
using UnchainexWallet.Blockchain.Transactions;

namespace UnchainexWallet.Fluent.Models;

public class TransactionAuthorizationInfo
{
	public TransactionAuthorizationInfo(BuildTransactionResult buildTransactionResult)
	{
		Psbt = buildTransactionResult.Psbt;
		Transaction = buildTransactionResult.Transaction;
	}

	public SmartTransaction Transaction { get; set; }

	public PSBT Psbt { get; }
}
