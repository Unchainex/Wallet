using System.Linq;
using NBitcoin;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Blockchain.TransactionBuilding;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.Wallets.Transactions.Inputs;
using UnchainexWallet.Fluent.ViewModels.Wallets.Transactions.Outputs;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Send;

public partial class TransactionSummaryViewModel : ViewModelBase
{
	private readonly IWalletModel _wallet;
	private BuildTransactionResult? _transaction;
	[AutoNotify] private bool _transactionHasChange;
	[AutoNotify] private TimeSpan? _confirmationTime;
	[AutoNotify] private string _feeText = "";
	[AutoNotify] private bool _isCustomFeeUsed;
	[AutoNotify] private bool _isOtherPocketSelectionPossible;
	[AutoNotify] private LabelsArray _labels = LabelsArray.Empty;
	[AutoNotify] private LabelsArray _recipient = LabelsArray.Empty;
	[AutoNotify] private Amount? _fee;
	[AutoNotify] private Amount? _amount;
	[AutoNotify] private FeeRate? _feeRate;
	[AutoNotify] private double? _amountDiff;
	[AutoNotify] private double? _feeDiff;
	[AutoNotify] private InputsCoinListViewModel? _inputList;
	[AutoNotify] private OutputsCoinListViewModel? _outputList;

	private TransactionSummaryViewModel(TransactionPreviewViewModel parent, IWalletModel wallet, TransactionInfo info, bool isPreview = false)
	{
		Parent = parent;
		_wallet = wallet;
		IsPreview = isPreview;
		AddressText = info.Destination.ToString(_wallet.Network);
		PayJoinUrl = info.PayJoinClient?.PaymentUrl.AbsoluteUri;
		IsPayJoin = PayJoinUrl is not null;
	}

	public TransactionPreviewViewModel Parent { get; }

	public bool IsPreview { get; }

	public string AddressText { get; }

	public string? PayJoinUrl { get; }

	public bool IsPayJoin { get; }

	public void UpdateTransaction(BuildTransactionResult transactionResult, TransactionInfo info)
	{
		_transaction = transactionResult;

		ConfirmationTime = _wallet.Transactions.TryEstimateConfirmationTime(info);

		var destinationAmount = _transaction.CalculateDestinationAmount(info.Destination);
		var destinationIndexedTxOut = transactionResult.Transaction.ForeignOutputs.Select(x => x.TxOut)
			.Union(transactionResult.Transaction.WalletOutputs.Select(x => x.TxOut))
			.FirstOrDefault(x =>
				info.Destination switch
				{
					Destination.Loudly loudly => x.ScriptPubKey == loudly.ScriptPubKey,
					Destination.Silent silent => false, /* we can only send */
					_ => throw new InvalidOperationException("Unknown destination type")
				});

		Amount = UiContext.AmountProvider.Create(destinationAmount);
		Fee = UiContext.AmountProvider.Create(_transaction.Fee);
		FeeRate = info.FeeRate;

		InputList = new InputsCoinListViewModel(
			transactionResult.Transaction.WalletInputs,
			_wallet.Network,
			transactionResult.Transaction.WalletInputs.Count + transactionResult.Transaction.ForeignInputs.Count,
			Parent.CurrentTransactionSummary.InputList?.TreeDataGridSource.Items.First().IsExpanded,
			!IsPreview ? null : Parent.CurrentTransactionSummary.InputList?.TreeDataGridSource.Items.First().Children.Count);

		OutputList = new OutputsCoinListViewModel(
			transactionResult.Transaction.WalletOutputs.Select(x => x.TxOut).ToList(),
			transactionResult.Transaction.ForeignOutputs.Select(x => x.TxOut).ToList(),
			_wallet.Network,
			destinationIndexedTxOut?.ScriptPubKey,
			Parent.CurrentTransactionSummary.OutputList?.TreeDataGridSource.Items.First().IsExpanded,
			!IsPreview ? null : Parent.CurrentTransactionSummary.OutputList?.TreeDataGridSource.Items.First().Children.Count);

		Recipient = info.Recipient;
		IsCustomFeeUsed = info.IsCustomFeeUsed;
		IsOtherPocketSelectionPossible = info.IsOtherPocketSelectionPossible;
		AmountDiff = DiffOrNull(Amount, Parent.CurrentTransactionSummary.Amount);
		FeeDiff = DiffOrNull(Fee, Parent.CurrentTransactionSummary.Fee);
	}

	private static double? DiffOrNull(Amount? current, Amount? previous)
	{
		if (current is null || previous is null)
		{
			return null;
		}

		return current.Diff(previous);
	}
}
