using System.Linq;
using NBitcoin;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Fluent.Models.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Transactions.Inputs;

public class InputsCoinViewModel : InputsCoinListItem
{
    public InputsCoinViewModel(SmartCoin coin, Network network)
	{
		Coin = coin;
		Amount = new Amount(coin.Amount);
		BtcAddress = coin.ScriptPubKey.GetDestinationAddress(network)?.ToString();
		if(coin.HdPubKey.HistoricalAnonSet.TryGetValue(coin.Outpoint.Hash, out var anonSetWhenTxProcessed))
		{
			AnonymityScore = (int)anonSetWhenTxProcessed;
		}
		else
		{
			AnonymityScore = (int)coin.AnonymitySet;
		}
	}

	public InputsCoinViewModel(InputsCoinViewModel[] coins, int inputCount, bool isExpanded, int? nbDiff)
	{
		Amount = new Amount(coins.Sum(x => x.Amount.Btc));
		Children = coins;
		TotalInputs = inputCount;
		IsExpanded = isExpanded;
		NbDiff = nbDiff;
		if (IsExpanded)
		{
			foreach (var coin in Children)
			{
				coin.IsExpanded = true;
			}
		}

		TitleText = $"{TotalInputs} input{(TotalInputs == 1 ? "" : "s")}";

		Tip = Children.Count == TotalInputs ?
			"All inputs belong to one of your opened wallets" :
			"Only inputs belonging to one of your opened wallets can be shown.";
	}
	public SmartCoin? Coin { get; }
}
