using UnchainexWallet.Blockchain.TransactionBuilding;
using UnchainexWallet.Blockchain.Transactions;

namespace UnchainexWallet.Fluent.Models.Wallets;

public record SpeedupTransaction(
	SmartTransaction TargetTransaction,
	BuildTransactionResult BoostingTransaction,
	bool AreWePayingTheFee,
	Amount Fee
	);
