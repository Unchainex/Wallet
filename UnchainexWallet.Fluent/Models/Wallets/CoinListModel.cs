using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Logging;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.Models.Wallets;

[AutoInterface]
public abstract partial class CoinListModel : IDisposable
{
	private readonly CompositeDisposable _disposables = new();

	public CoinListModel(Wallet wallet, IWalletModel walletModel)
	{
		Wallet = wallet;
		WalletModel = walletModel;
		var transactionProcessed = walletModel.Transactions.TransactionProcessed;
		var anonScoreTargetChanged = this.WhenAnyValue(x => x.WalletModel.Settings.AnonScoreTarget).Skip(1).ToSignal();
		var isCoinjoinRunningChanged = walletModel.IsCoinjoinRunning.ToSignal();
		var isSelected = this.WhenAnyValue(x => x.WalletModel.IsSelected).Skip(1).ToSignal();

		var signals =
			transactionProcessed
				.Merge(anonScoreTargetChanged)
				.Merge(isCoinjoinRunningChanged)
				.Merge(isSelected)
				.Publish();

		List = signals.Fetch(CreateCoinModels, x => x.Key).DisposeWith(_disposables);
		Pockets = signals.Fetch(GetPockets, x => x.Labels).DisposeWith(_disposables);

		signals
			.Do(_ => Logger.LogDebug($"Refresh signal emitted in {walletModel.Name}"))
			.Subscribe()
			.DisposeWith(_disposables);

		signals.Connect()
			.DisposeWith(_disposables);
	}

	protected Wallet Wallet { get; }
	protected IWalletModel WalletModel { get; }

	public IObservableCache<ICoinModel, int> List { get; }

	public IObservableCache<Pocket, LabelsArray> Pockets { get; }

	public ICoinModel GetCoinModel(SmartCoin smartCoin)
	{
		return List.Items.First(coinModel => coinModel.Key == smartCoin.Outpoint.GetHashCode());
	}

	protected ICoinModel CreateCoinModel(SmartCoin smartCoin)
	{
		return new CoinModel(smartCoin, WalletModel.Network, WalletModel.Settings.AnonScoreTarget);
	}

	protected abstract Pocket[] GetPockets();

	protected abstract ICoinModel[] CreateCoinModels();

	public void Dispose() => _disposables.Dispose();
}
