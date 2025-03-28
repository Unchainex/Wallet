using DynamicData;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Fluent.Models;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Hwi.Models;
using UnchainexWallet.Models;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Tests.UnitTests.ViewModels.UIContext;

public class NullWalletRepository : IWalletRepository
{
	public NullWalletRepository()
	{
		Wallets = Array.Empty<IWalletModel>().AsObservableChangeSet(x => x.Id).AsObservableCache();
	}

	public IObservableCache<IWalletModel, WalletId> Wallets { get; }

	public string? DefaultWalletName => null;

	public bool HasWallet => false;

	public IWalletModel GetExistingWallet(HwiEnumerateEntry device)
	{
		throw new NotSupportedException();
	}

	public string GetNextWalletName()
	{
		return "Wallet";
	}

	public Task<IWalletSettingsModel> NewWalletAsync(WalletCreationOptions options, CancellationToken? cancelToken = null)
	{
		return Task.FromResult(default(IWalletSettingsModel)!);
	}

	public IWalletModel SaveWallet(IWalletSettingsModel walletSettings)
	{
		return default!;
	}

	public (ErrorSeverity Severity, string Message)? ValidateWalletName(string walletName)
	{
		return null;
	}

	public void StoreLastSelectedWallet(IWalletModel wallet)
	{
	}
}
