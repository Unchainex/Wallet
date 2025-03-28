using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Blockchain.TransactionBuilding;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.ViewModels.Wallets.Send;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.Models.Wallets;

[AutoInterface]
public partial class WalletCoinsModel(Wallet wallet, IWalletModel walletModel) : CoinListModel(wallet, walletModel)
{
	public async Task UpdateExcludedCoinsFromCoinjoinAsync(ICoinModel[] coinsToExclude)
	{
		await Task.Run(() =>
		{
			var outPoints = coinsToExclude.Select(x => x.GetSmartCoin().Outpoint).ToArray();
			Wallet.UpdateExcludedCoinsFromCoinJoin(outPoints);
		});
	}

	public List<ICoinModel> GetSpentCoins(BuildTransactionResult? transaction)
	{
		var coins = (transaction?.SpentCoins ?? new List<SmartCoin>()).ToList();
		return coins.Select(GetCoinModel).ToList();
	}

	public bool AreEnoughToCreateTransaction(TransactionInfo transactionInfo, IEnumerable<ICoinModel> coins)
	{
		return TransactionHelpers.TryBuildTransactionWithoutPrevTx(Wallet.KeyManager, transactionInfo, Wallet.Coins, coins.GetSmartCoins(), Wallet.Password, out _);
	}

	protected override Pocket[] GetPockets()
	{
		return Wallet.GetPockets().ToArray();
	}

	protected override ICoinModel[] CreateCoinModels()
	{
		return Wallet.Coins.Select(CreateCoinModel).ToArray();
	}
}
