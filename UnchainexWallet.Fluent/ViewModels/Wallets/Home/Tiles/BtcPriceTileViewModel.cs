using System.Reactive.Linq;
using ReactiveUI;
using UnchainexWallet.Fluent.Models.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Home.Tiles;

public partial class BtcPriceTileViewModel : ActivatableViewModel
{
	[AutoNotify] private decimal _usdPerBtc;

	public BtcPriceTileViewModel(IAmountProvider amountProvider)
	{
		amountProvider.BtcToUsdExchangeRate
			.ObserveOn(RxApp.MainThreadScheduler)
			.StartWith(amountProvider.UsdExchangeRate)
			.Subscribe(x => UsdPerBtc = x);
	}
}
