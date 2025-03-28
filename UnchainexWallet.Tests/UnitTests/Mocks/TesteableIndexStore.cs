using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Models;
using UnchainexWallet.Stores;

namespace UnchainexWallet.Tests.UnitTests.Mocks;

class TesteableIndexStore : IIndexStore
{
	public Func<uint, int, CancellationToken, Task<FilterModel[]>> OnFetchBatchAsync { get; set; }
	public Task<FilterModel[]> FetchBatchAsync(uint fromHeight, int batchSize, CancellationToken cancellationToken) =>
		OnFetchBatchAsync.Invoke(fromHeight, batchSize, cancellationToken);
}
