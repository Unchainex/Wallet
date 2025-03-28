using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Models;

namespace UnchainexWallet.Interfaces;

public interface IExchangeRateProvider
{
	Task<IEnumerable<ExchangeRate>> GetExchangeRateAsync(CancellationToken cancellationToken);
}
