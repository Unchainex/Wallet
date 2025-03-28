using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Helpers;
using UnchainexWallet.Fluent.Models;
using UnchainexWallet.Blockchain.Transactions;

namespace UnchainexWallet.Fluent.Helpers;

public static class CoinHelpers
{
	public static LabelsArray GetLabels(this SmartCoin coin, int privateThreshold)
	{
		if (coin.IsPrivate(privateThreshold) || coin.IsSemiPrivate(privateThreshold))
		{
			return LabelsArray.Empty;
		}

		if (coin.HdPubKey.Cluster.Labels == LabelsArray.Empty)
		{
			return CoinPocketHelper.UnlabelledFundsText;
		}

		return coin.HdPubKey.Cluster.Labels;
	}

	public static int GetConfirmations(this SmartCoin coin) => coin.Transaction.GetConfirmations((int)Services.SmartHeaderChain.TipHeight);

	public static PrivacyLevel GetPrivacyLevel(this SmartCoin coin, int privateThreshold)
	{
		if (coin.IsPrivate(privateThreshold))
		{
			return PrivacyLevel.Private;
		}

		if (coin.IsSemiPrivate(privateThreshold))
		{
			return PrivacyLevel.SemiPrivate;
		}

		return PrivacyLevel.NonPrivate;
	}
}
