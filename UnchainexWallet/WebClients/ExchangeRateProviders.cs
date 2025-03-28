using System.Collections.Generic;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Models;
using UnchainexWallet.Interfaces;
using UnchainexWallet.Logging;
using UnchainexWallet.WebClients.BlockchainInfo;
using UnchainexWallet.WebClients.Coinbase;
using UnchainexWallet.WebClients.Bitstamp;
using UnchainexWallet.WebClients.CoinGecko;
using UnchainexWallet.WebClients.Gemini;
using System.Linq;
using System.Threading;
using UnchainexWallet.WebClients.Coingate;

namespace UnchainexWallet.WebClients;

public class ExchangeRateProvider : IExchangeRateProvider
{
	private readonly IExchangeRateProvider[] _exchangeRateProviders =
	{
		new BlockchainInfoExchangeRateProvider(),
		new BitstampExchangeRateProvider(),
		new CoinGeckoExchangeRateProvider(),
		new CoinbaseExchangeRateProvider(),
		new GeminiExchangeRateProvider(),
		new CoingateExchangeRateProvider()
	};

	public async Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken)
	{
		foreach (var provider in _exchangeRateProviders)
		{
			try
			{
				return await provider.GetExchangeRateAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				// Ignore it and try with the next one
				Logger.LogTrace(ex);
			}
		}
		return Enumerable.Empty<ExchangeRate>();
	}
}
