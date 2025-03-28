using ReactiveUI;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.Models.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Home.History.HistoryItems;

public partial class CoinJoinHistoryItemViewModel : HistoryItemViewModelBase
{
	private CoinJoinHistoryItemViewModel(IWalletModel wallet, TransactionModel transaction) : base(transaction)
	{
		ShowDetailsCommand = ReactiveCommand.Create(() => UiContext.Navigate().To().CoinJoinDetails(wallet, transaction));
	}
}
