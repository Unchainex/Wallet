using NBitcoin;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Blockchain.TransactionProcessing;
using UnchainexWallet.FeeRateEstimation;
using UnchainexWallet.Models;
using UnchainexWallet.Services;
using UnchainexWallet.Stores;
using UnchainexWallet.Wallets.FilterProcessor;

namespace UnchainexWallet.Wallets;

/// <summary>
/// Class to create <see cref="Wallet"/> instances.
/// </summary>
public record WalletFactory(
	string DataDir,
	Network Network,
	BitcoinStore BitcoinStore,
	ServiceConfiguration ServiceConfiguration,
	FeeRateEstimationUpdater FeeRateEstimationUpdater,
	BlockDownloadService BlockDownloadService,
    CpfpInfoProvider? CpfpInfoProvider = null)
{
	public Wallet Create(KeyManager keyManager)
	{
		TransactionProcessor transactionProcessor = new(BitcoinStore.TransactionStore, BitcoinStore.MempoolService, keyManager, ServiceConfiguration.DustThreshold);
		WalletFilterProcessor walletFilterProcessor = new(keyManager, BitcoinStore, transactionProcessor, BlockDownloadService);

		return new(DataDir, Network, keyManager, BitcoinStore, ServiceConfiguration, FeeRateEstimationUpdater, transactionProcessor, walletFilterProcessor, CpfpInfoProvider);
	}

	public Wallet CreateAndInitialize(KeyManager keyManager)
	{
		Wallet wallet = Create(keyManager);
		wallet.Initialize();

		return wallet;
	}
}
