using UnchainexWallet.Blockchain.TransactionBuilding;

namespace UnchainexWallet.Fluent.Models.Wallets;

public record CancellingTransaction(
	TransactionModel TargetTransaction,
	BuildTransactionResult CancelTransaction,
	Amount Fee);
