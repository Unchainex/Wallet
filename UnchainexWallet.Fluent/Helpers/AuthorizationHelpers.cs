using UnchainexWallet.Blockchain.TransactionBuilding;
using UnchainexWallet.Fluent.Models;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Authorization;

namespace UnchainexWallet.Fluent.Helpers;

// TODO: Remove this entire class after SendViewModel is decoupled.
public static class AuthorizationHelpers
{
	public static AuthorizationDialogBase GetAuthorizationDialog(IWalletModel wallet, BuildTransactionResult transaction)
	{
		var transactionAuthorizationInfo = new TransactionAuthorizationInfo(transaction);
		return GetAuthorizationDialog(wallet, transactionAuthorizationInfo);
	}

	public static AuthorizationDialogBase GetAuthorizationDialog(IWalletModel wallet, TransactionAuthorizationInfo transactionAuthorizationInfo)
	{
		if (wallet is IHardwareWalletModel hwm)
		{
			return new HardwareWalletAuthDialogViewModel(hwm, transactionAuthorizationInfo);
		}
		else
		{
			return new PasswordAuthDialogViewModel(wallet, "Send");
		}
	}
}
