using System.Collections.Generic;
using System.Linq;
using UnchainexWallet.Fluent.ViewModels.Wallets.Labels;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.Helpers;

public static class LabelHelpers
{
	public static IEnumerable<(string Label, int Score)> GetLabelsWithRanking(this Wallet wallet, Intent intent)
	{
		var labelPool = new Dictionary<string, int>(); // int: score.

		var labelsByWalletId = WalletHelpers.GetLabelsByWallets();

		// Make recent and receive labels count more for the current wallet.
		var multiplier = 100;
		var currentWalletReceiveLabels = labelsByWalletId.First(x => x.WalletId == wallet.WalletId).ReceiveLabels;
		for (var i = currentWalletReceiveLabels.Count - 1; i >= 0; i--) // Iterate in reverse order.
		{
			var label = currentWalletReceiveLabels[i];
			var score = (intent == Intent.Receive ? 100 : 1) * multiplier;
			if (!labelPool.TryAdd(label, score))
			{
				labelPool[label] += score;
			}

			if (multiplier > 1)
			{
				multiplier--;
			}
		}

		// Receive addresses should be more dominant.
		foreach (var label in labelsByWalletId.SelectMany(x => x.ReceiveLabels))
		{
			var score = intent == Intent.Receive ? 100 : 1;
			if (!labelPool.TryAdd(label, score))
			{
				labelPool[label] += score;
			}
		}

		// Change addresses shouldn't be much dominant, but should be present.
		foreach (var label in labelsByWalletId.SelectMany(x => x.ChangeLabels))
		{
			var score = 1;
			if (!labelPool.TryAdd(label, score))
			{
				labelPool[label] += score;
			}
		}

		multiplier = 100; // Make recent labels count more.
		foreach (var label in WalletHelpers.GetTransactionLabels().SelectMany(x => x).Reverse())
		{
			var score = (intent == Intent.Send ? 100 : 1) * multiplier;
			if (!labelPool.TryAdd(label, score))
			{
				labelPool[label] += score;
			}

			if (multiplier > 1)
			{
				multiplier--;
			}
		}

		var unwantedLabelSuggestions = new[]
		{
			"test", // Often people use the string "test" as a label. It obviously cannot be a real label, just a test label.
			"zerolink mixed coin", // Obsoleted autogenerated label from old WW1 versions.
			"zerolink change", // Obsoleted autogenerated label from old WW1 versions.
			"zerolink dequeued change" // Obsoleted autogenerated label from old WW1 versions.
		};

		var labels = labelPool
			.Where(x =>
				!unwantedLabelSuggestions.Any(y => y.Equals(x.Key, StringComparison.OrdinalIgnoreCase))
				&& !x.Key.StartsWith("change of (", StringComparison.OrdinalIgnoreCase)); // An obsoleted autogenerated label pattern was from old WW1 versions starting with "change of (".

		var mostUsedLabels = labels
			.GroupBy(x => x.Key)
			.Select(x => new
			{
				Label = x.Key,
				Score = x.Sum(y => y.Value)
			})
			.OrderByDescending(x => x.Score)
			.ToList();

		return mostUsedLabels.Select(x => (x.Label, x.Score)).ToList();
	}
}
