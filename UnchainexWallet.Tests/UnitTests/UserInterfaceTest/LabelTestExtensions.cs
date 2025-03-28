using System.Collections.Generic;
using System.Linq;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Fluent.Models;
using UnchainexWallet.Fluent.ViewModels.Wallets.Send;
using UnchainexWallet.Tests.Helpers;

namespace UnchainexWallet.Tests.UnitTests.UserInterfaceTest;

internal static class LabelTestExtensions
{
	private static readonly KeyManager KeyManager = ServiceFactory.CreateKeyManager();

	public static LabelViewModel GetLabel(this LabelSelectionViewModel selection, string label)
	{
		return selection.AllLabelsViewModel.Single(x => x.Value == label);
	}

	public static void AddPocket(this List<Pocket> pockets, decimal amount, out Pocket pocket, params string[] labels)
	{
		var labelsArray = new LabelsArray(labels);
		var coinsView = new CoinsView(new[] { BitcoinFactory.CreateSmartCoin(NewKey(labelsArray), amount) });
		pocket = new Pocket((labelsArray, coinsView));
		pockets.Add(pocket);
	}

	public static HdPubKey NewKey(string label = "", int anonymitySet = 1)
	{
		var key = KeyManager.GenerateNewKey(label, KeyState.Used, true);
		key.SetAnonymitySet(anonymitySet);
		key.SetLabel(label);

		return key;
	}

	public static SmartCoin CreateCoin(decimal amount, string label = "", int anonymitySet = 1)
	{
		var coin = BitcoinFactory.CreateSmartCoin(NewKey(label: label, anonymitySet: anonymitySet), amount);
		coin.HdPubKey.SetAnonymitySet(anonymitySet);

		return coin;
	}

	public static Pocket CreateSingleCoinPocket(decimal amount, string label = "", int anonSet = 0)
	{
		var coins = new[]
		{
			CreateCoin(amount, label, anonSet)
		};

		var coinsView = new CoinsView(coins.ToArray());
		var pocket = new Pocket((label, coinsView));

		return pocket;
	}
}
