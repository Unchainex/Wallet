using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.Navigation;
using UnchainexWallet.Fluent.ViewModels.Wallets.Coins;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Advanced;

[NavigationMetaData(
	Title = "Wallet Coins",
	Caption = "Display wallet coins",
	IconName = "nav_wallet_24_regular",
	Order = 0,
	Category = "Wallet",
	Keywords = ["Wallet", "Coins", "UTXO"],
	NavBarPosition = NavBarPosition.None,
	NavigationTarget = NavigationTarget.DialogScreen,
	Searchable = false)]
public partial class WalletCoinsViewModel : RoutableViewModel
{
	private readonly IWalletModel _wallet;

	public WalletCoinsViewModel(UiContext uiContext, IWalletModel wallet)
	{
		UiContext = uiContext;
		_wallet = wallet;
		SetupCancel(enableCancel: false, enableCancelOnEscape: true, enableCancelOnPressed: true);
		NextCommand = CancelCommand;
		CoinList = new CoinListViewModel(_wallet.Coins, new List<ICoinModel>(), allowCoinjoiningCoinSelection: false, ignorePrivacyMode: false, allowSelection: false);
	}

	public CoinListViewModel CoinList { get; }

	protected override void OnNavigatedTo(bool isInHistory, CompositeDisposable disposables)
	{
		if (!isInHistory)
		{
			CoinList.ExpandAllCommand.Execute().Subscribe().DisposeWith(disposables);
		}
	}

	protected override void OnNavigatedFrom(bool isInHistory)
	{
		if (!isInHistory)
		{
			CoinList.Dispose();
		}
	}
}
