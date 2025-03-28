using ReactiveUI;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Logging;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Wallets;

public class HardwareWalletViewModel : WalletViewModel
{
	internal HardwareWalletViewModel(UiContext uiContext, IWalletModel walletModel, Wallet wallet) : base(uiContext, walletModel, wallet)
	{
		BroadcastPsbtCommand = ReactiveCommand.CreateFromTask(async () =>
		{
			try
			{
				var file = await FileDialogHelper.OpenFileAsync("Import Transaction", new[] { "psbt", "txn", "*" });
				if (file is { })
				{
					var path = file.Path.LocalPath;
					var txn = await walletModel.Transactions.LoadFromFileAsync(path);
					Navigate().To().BroadcastTransaction(txn);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
				await ShowErrorAsync(Title, ex.ToUserFriendlyString(), "It was not possible to load the transaction.");
			}
		});
	}
}
