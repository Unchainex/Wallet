using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.TreeDataGrid;
using UnchainexWallet.Fluent.ViewModels.CoinControl;
using UnchainexWallet.Fluent.Views.CoinControl.Core.Cells;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Coins;

public static class CoinListDataGridSource
{
	public static HierarchicalTreeDataGridSource<CoinListItem> Create(IEnumerable<CoinListItem> source, bool allowSelection)
	{
		// [Column]			[View]					[Header]	[Width]		[MinWidth]		[MaxWidth]	[CanUserSort]
		// Indicators		IndicatorsColumnView	-			Auto		-				-			true
		// AnonymityScore	AnonymityColumnView		<custom>	50			-				-			true
		// Amount			AmountColumnView		Amount		Auto		-				-			true
		// Labels			LabelsColumnView		Labels		*			-				-			true
		// Selection		SelectionColumnView		-			Auto		-				-			false
		var result = new HierarchicalTreeDataGridSource<CoinListItem>(source)
		{
			Columns =
			{
				IndicatorsColumn(),
				AnonymityScoreColumn(),
				AmountColumn(),
				LabelsColumn(),
			}
		};

		if (allowSelection)
		{
			result.Columns.Add(SelectionColumn());
		}

		return result;
	}

	private static int GetIndicatorPriority(CoinListItem x)
	{
		if (x.IsCoinjoining)
		{
			return 1;
		}

		if (x.BannedUntilUtc.HasValue)
		{
			return 2;
		}

		if (!x.IsConfirmed)
		{
			return 3;
		}

		if (!x.IsExcludedFromCoinJoin)
		{
			return 4;
		}

		return 0;
	}

	private static IColumn<CoinListItem> IndicatorsColumn()
	{
		return new HierarchicalExpanderColumn<CoinListItem>(
			new TemplateColumn<CoinListItem>(
				null,
				new FuncDataTemplate<CoinListItem>((_, _) => new IndicatorsCellView(), true),
				null,
				GridLength.Auto,
				new TemplateColumnOptions<CoinListItem>
				{
					CompareAscending = Sort<CoinListItem>.Ascending(GetIndicatorPriority),
					CompareDescending = Sort<CoinListItem>.Descending(GetIndicatorPriority)
				}),
			group => group.Children,
			node => node.HasChildren(),
			node => node.IsExpanded);
	}

	private static TemplateColumn<CoinListItem> SelectionColumn()
	{
		return new TemplateColumn<CoinListItem>(
			null,
			new FuncDataTemplate<CoinListItem>(
				(_, _) => new SelectionCellView(),
				true),
			null,
			GridLength.Auto);
	}

	private static IColumn<CoinListItem> AmountColumn()
	{
		return new TemplateColumn<CoinListItem>(
			null,
			new FuncDataTemplate<CoinListItem>((_, _) => new AmountCellView(), true),
			null,
			GridLength.Star,
			new TemplateColumnOptions<CoinListItem>
			{
				CompareAscending = Sort<CoinListItem>.Ascending(x => x.Amount.Btc),
				CompareDescending = Sort<CoinListItem>.Descending(x => x.Amount.Btc)
			});
	}

	private static IColumn<CoinListItem> AnonymityScoreColumn()
	{
		return new TemplateColumn<CoinListItem>(
			null,
			new FuncDataTemplate<CoinListItem>((_, _) => new AnonymityScoreCellView(), true),
			null,
			GridLength.Auto,
			new TemplateColumnOptions<CoinListItem>
			{
				CompareAscending = Sort<CoinListItem>.Ascending(b => b.AnonymityScore ?? b.Children.Min(x => x.AnonymityScore)),
				CompareDescending = Sort<CoinListItem>.Descending(b => b.AnonymityScore ?? b.Children.Min(x => x.AnonymityScore))
			});
	}

	private static IColumn<CoinListItem> LabelsColumn()
	{
		return new TemplateColumn<CoinListItem>(
			null,
			new FuncDataTemplate<CoinListItem>((_, _) => new LabelsCellView(), true),
			null,
			GridLength.Star,
			new TemplateColumnOptions<CoinListItem>
			{
				CompareAscending = CoinControlLabelComparer.Ascending,
				CompareDescending = CoinControlLabelComparer.Descending
			});
	}
}
