using NBitcoin;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Extensions;
using UnchainexWallet.Helpers;
using UnchainexWallet.Stores;

namespace UnchainexWallet.Blockchain.Transactions;

public class AllTransactionStore : ITransactionStore, IAsyncDisposable
{
	/// <param name="dataSource">Work folder, or <see cref="SqliteStorageHelper.InMemoryDatabase"/> to use an empty in-memory database in tests.</param>
	public AllTransactionStore(string dataSource, Network network)
	{
		_workFolderPath = dataSource;

		string mempoolStoreDataSource;
		string confirmedStoreDataSource;

		if (dataSource == SqliteStorageHelper.InMemoryDatabase)
		{
			mempoolStoreDataSource = SqliteStorageHelper.InMemoryDatabase;
			confirmedStoreDataSource = SqliteStorageHelper.InMemoryDatabase;
		}
		else
		{
			IoHelpers.EnsureDirectoryExists(_workFolderPath);
			mempoolStoreDataSource = Path.Combine(_workFolderPath, "Mempool");
			confirmedStoreDataSource = Path.Combine(_workFolderPath, "ConfirmedTransactions", Constants.ConfirmedTransactionsVersion);
		}

		MempoolStore = new TransactionStore(workFolderPath: mempoolStoreDataSource, network);
		ConfirmedStore = new TransactionStore(workFolderPath: confirmedStoreDataSource, network);
	}

	private readonly string _workFolderPath;

	public TransactionStore MempoolStore { get; }
	public TransactionStore ConfirmedStore { get; }
	private readonly object _lock = new();

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var initTasks = new[]
		{
			MempoolStore.InitializeAsync($"{nameof(MempoolStore)}.{nameof(MempoolStore.InitializeAsync)}", cancellationToken),
			ConfirmedStore.InitializeAsync($"{nameof(ConfirmedStore)}.{nameof(ConfirmedStore.InitializeAsync)}", cancellationToken)
		};

		await Task.WhenAll(initTasks).ConfigureAwait(false);
		EnsureConsistency();
	}

	public void AddOrUpdate(SmartTransaction tx)
	{
		lock (_lock)
		{
			var hash = tx.GetHash();

			if (tx.Confirmed)
			{
				if (MempoolStore.TryRemove(hash, out var found))
				{
					found.TryUpdate(tx);
					ConfirmedStore.TryAddOrUpdate(found);
				}
				else
				{
					ConfirmedStore.TryAddOrUpdate(tx);
				}
			}
			else
			{
				if (!ConfirmedStore.TryUpdate(tx))
				{
					MempoolStore.TryAddOrUpdate(tx);
				}
			}
		}
	}

	internal bool TryUpdate(SmartTransaction tx)
	{
		uint256 hash = tx.GetHash();

		lock (_lock)
		{
			// Do Contains first, because it's fast.
			if (ConfirmedStore.TryUpdate(tx))
			{
				return true;
			}
			else if (tx.Confirmed && MempoolStore.TryRemove(hash, out SmartTransaction? originalTx))
			{
				originalTx.TryUpdate(tx);
				ConfirmedStore.TryAddOrUpdate(originalTx);
				return true;
			}
			else if (MempoolStore.TryUpdate(tx))
			{
				return true;
			}
		}

		return false;
	}

	private void EnsureConsistency()
	{
		lock (_lock)
		{
			List<uint256> mempoolTransactions = MempoolStore.GetTransactionHashes();

			foreach (uint256 txid in mempoolTransactions)
			{
				// Contains is fast, so do this first.
				if (ConfirmedStore.Contains(txid)
					&& MempoolStore.TryRemove(txid, out var uTx))
				{
					ConfirmedStore.TryAddOrUpdate(uTx);
				}
			}
		}
	}

	public virtual bool TryGetTransaction(uint256 hash, [NotNullWhen(true)] out SmartTransaction? sameStx)
	{
		lock (_lock)
		{
			if (MempoolStore.TryGetTransaction(hash, out sameStx))
			{
				return true;
			}

			return ConfirmedStore.TryGetTransaction(hash, out sameStx);
		}
	}

	/// <returns>Transactions ordered by blockchain.</returns>
	public IEnumerable<SmartTransaction> GetTransactions()
	{
		lock (_lock)
		{
			return ConfirmedStore.GetTransactions().Concat(MempoolStore.GetTransactions()).OrderByBlockchain().ToList();
		}
	}

	internal IEnumerable<uint256> GetTransactionHashes()
	{
		lock (_lock)
		{
			return ConfirmedStore.GetTransactionHashes().Concat(MempoolStore.GetTransactionHashes()).ToList();
		}
	}

	internal bool IsEmpty()
	{
		lock (_lock)
		{
			return ConfirmedStore.IsEmpty() && MempoolStore.IsEmpty();
		}
	}

	/// <remarks>Only used by tests.</remarks>
	internal bool Contains(uint256 hash)
	{
		lock (_lock)
		{
			return ConfirmedStore.Contains(hash) || MempoolStore.Contains(hash);
		}
	}

	public IEnumerable<SmartTransaction> ReleaseToMempoolFromBlock(uint256 blockHash)
	{
		lock (_lock)
		{
			List<SmartTransaction> reorgedTxs = new();
			foreach (var txHash in ConfirmedStore
				.GetTransactions()
				.Where(tx => tx.BlockHash == blockHash)
				.Select(tx => tx.GetHash()))
			{
				if (ConfirmedStore.TryRemove(txHash, out var removedTx))
				{
					removedTx.SetUnconfirmed();

					if (MempoolStore.TryAdd(removedTx))
					{
						reorgedTxs.Add(removedTx);
					}
					else
					{
						MempoolStore.TryUpdate(removedTx);
					}
				}
			}
			return reorgedTxs;
		}
	}

	/// <returns>Labels ordered by blockchain.</returns>
	public IEnumerable<LabelsArray> GetLabels() => GetTransactions().Select(x => x.Labels);

	public async ValueTask DisposeAsync()
	{
		await MempoolStore.DisposeAsync().ConfigureAwait(false);
		await ConfirmedStore.DisposeAsync().ConfigureAwait(false);
	}
}
