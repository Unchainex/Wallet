using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NBitcoin;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Helpers;
using UnchainexWallet.Userfacing;

namespace UnchainexWallet.Fluent.Infrastructure;

internal class ClipboardObserver
{
	private readonly IObservable<Amount> _balances;

	public ClipboardObserver(IObservable<Amount> balances)
	{
		_balances = balances;
	}

	public IObservable<string?> ClipboardUsdContentChanged(IScheduler scheduler)
	{
		return ApplicationHelper.ClipboardTextChanged(scheduler)
			.CombineLatest(_balances.Select(x => x.Usd).Switch(), ParseToUsd)
			.Select(money => money?.ToString("0.00"));
	}

	public IObservable<string?> ClipboardBtcContentChanged(IScheduler scheduler)
	{
		return ApplicationHelper.ClipboardTextChanged(scheduler)
			.CombineLatest(_balances.Select(x => x.Btc), ParseToMoney);
	}

	public static decimal? ParseToUsd(string? text)
	{
		if (text is null)
		{
			return null;
		}

		if (CurrencyInput.TryCorrectAmount(text, out var corrected))
		{
			text = corrected;
		}

		return decimal.TryParse(text, CurrencyInput.InvariantNumberFormat, out var n) ? n : (decimal?)default;
	}

	public static decimal? ParseToUsd(string? text, decimal balanceUsd)
	{
		return ParseToUsd(text)
			.Ensure(n => n <= balanceUsd)
			.Ensure(n => n >= 1)
			.Ensure(n => n.CountDecimalPlaces() <= 2);
	}

	public static Money? ParseToMoney(string? text)
	{
		if (text is null)
		{
			return null;
		}

		if (CurrencyInput.TryCorrectBitcoinAmount(text, out var corrected))
		{
			text = corrected;
		}

		return Money.TryParse(text, out var n) ? n : default;
	}

	public static string? ParseToMoney(string? text, Money balance)
	{
		// Ignore paste if there are invalid characters
		if (text is null || !CurrencyInput.RegexValidCharsOnly().IsMatch(text))
		{
			return null;
		}

		if (CurrencyInput.TryCorrectBitcoinAmount(text, out var corrected))
		{
			text = corrected;
		}	

		var money = ParseToMoney(text).Ensure(m => m <= balance);
		return money?.ToDecimal(MoneyUnit.BTC).FormattedBtcExactFractional(text);
	}
}
