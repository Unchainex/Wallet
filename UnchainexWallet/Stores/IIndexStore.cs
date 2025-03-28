using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Models;

namespace UnchainexWallet.Stores;

public interface IIndexStore
{
	Task<FilterModel[]> FetchBatchAsync(uint fromHeight, int batchSize, CancellationToken cancellationToken);
}
