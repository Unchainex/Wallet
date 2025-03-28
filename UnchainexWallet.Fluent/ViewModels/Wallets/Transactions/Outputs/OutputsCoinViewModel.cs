using System.Linq;
using NBitcoin;
using UnchainexWallet.Fluent.Models.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Transactions.Outputs;

public class OutputsCoinViewModel : OutputsCoinListItem
{
    public OutputsCoinViewModel(TxOut txOut, Network network, bool isOwn, bool isChange)
	{
		TxOut = txOut;
		Amount = new Amount(txOut.Value);
		BtcAddress = txOut.ScriptPubKey.GetDestinationAddress(network)?.ToString();
		ShowChange = isChange;
		ShowOwn = !isChange && isOwn;
	}

	public OutputsCoinViewModel(OutputsCoinViewModel[] coins, int outputCount, bool isExpanded, int? nbDiff)
	{
		Amount = new Amount(coins.Sum(x => x.Amount.Btc));
		Children = coins;
		TotalOutputs = outputCount;
		IsExpanded = isExpanded;
		if (IsExpanded)
		{
			foreach (var coin in Children)
			{
				coin.IsExpanded = true;
			}
		}
		TitleText = $"{Children.Count} output{(Children.Count == 1 ? "" : "s")}";
		NbDiff = nbDiff;
	}
	public TxOut? TxOut { get; }
}
