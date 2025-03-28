using System.Reactive.Linq;
using NBitcoin;
using ReactiveUI;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Services;

namespace UnchainexWallet.Fluent.Models.Wallets;

[AutoInterface]
public partial class AmountProvider : ReactiveObject
{
	[AutoNotify] private decimal _usdExchangeRate;

	public AmountProvider()
	{
		BtcToUsdExchangeRate = Services.EventBus
			.AsObservable<ExchangeRateChanged>()
			.ObserveOn(RxApp.MainThreadScheduler)
			.Select(x =>
				x.UsdBtcRate
				);

		BtcToUsdExchangeRate.Subscribe(x =>
			UsdExchangeRate = x
			);
	}

	public IObservable<decimal> BtcToUsdExchangeRate { get; }

	public Amount Create(Money? money)
	{
		return new Amount(money ?? Money.Zero, this);
	}
}
