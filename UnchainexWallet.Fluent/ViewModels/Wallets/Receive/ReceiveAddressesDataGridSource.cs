using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.Views.Wallets.Receive.Columns;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Receive;

public static class ReceiveAddressesDataGridSource
{
	// [Column]		[View]				  [Header]  	  [Width]		[MinWidth]		[MaxWidth]	[CanUserSort]
	// ScriptType   ScriptTypeColumnView  Type            Auto          -               -           false
	// Address		AddressColumnView	  Address		  Auto		    -				-			true
	// Labels		LabelsColumnView	  Labels		  1*			-				-			false
	// Actions		ActionsColumnView	  -			      Auto		    -				-			false
	public static FlatTreeDataGridSource<AddressViewModel> Create(IEnumerable<AddressViewModel> addresses)
	{
		return new FlatTreeDataGridSource<AddressViewModel>(addresses)
		{
			Columns =
			{
				ScriptTypeColumn(),
				AddressColumn(),
				LabelsColumn(),
				ActionsColumn(),
			}
		};
	}

	private static IColumn<AddressViewModel> ActionsColumn()
	{
		return new TemplateColumn<AddressViewModel>(
			null,
			new FuncDataTemplate<AddressViewModel>((node, ns) => new ActionsColumnView(), true),
			null,
			options: new TemplateColumnOptions<AddressViewModel>
			{
				CanUserResizeColumn = false,
				CanUserSortColumn = false
			},
			width: new GridLength(0, GridUnitType.Auto));
	}

	private static IColumn<AddressViewModel> AddressColumn()
	{
		return new TemplateColumn<AddressViewModel>(
			"Address",
			new FuncDataTemplate<AddressViewModel>((_, _) => new AddressColumnView(), true),
			null,
			options: new TemplateColumnOptions<AddressViewModel>
			{
				CanUserResizeColumn = false,
				CanUserSortColumn = true,
				CompareAscending = Sort<AddressViewModel>.Ascending(x => x.AddressText),
				CompareDescending = Sort<AddressViewModel>.Descending(x => x.AddressText)
			},
			width: new GridLength(0, GridUnitType.Auto));
	}

	private static IColumn<AddressViewModel> LabelsColumn()
	{
		return new TemplateColumn<AddressViewModel>(
			"Labels",
			new FuncDataTemplate<AddressViewModel>((_, _) => new LabelsColumnView(), true),
			null,
			options: new TemplateColumnOptions<AddressViewModel>
			{
				CanUserResizeColumn = false,
				CanUserSortColumn = true,
				CompareAscending = Sort<AddressViewModel>.Ascending(x => x.Labels),
				CompareDescending = Sort<AddressViewModel>.Descending(x => x.Labels)
			},
			width: new GridLength(1, GridUnitType.Star));
	}

	private static IColumn<AddressViewModel> ScriptTypeColumn()
	{
		return new TemplateColumn<AddressViewModel>(
			"Type",
			new FuncDataTemplate<AddressViewModel>((_, _) => new ScriptTypeView(), true),
			null,
			options: new TemplateColumnOptions<AddressViewModel>
			{
				CanUserResizeColumn = false,
				CanUserSortColumn = false
			},
			width: new GridLength(0, GridUnitType.Auto));
	}
}
