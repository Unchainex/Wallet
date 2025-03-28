using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using UnchainexWallet.Fluent.Controls.Sorting;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.Navigation;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Receive;

[NavigationMetaData(Title = "Addresses Awaiting Payment")]
public partial class ReceiveAddressesViewModel : RoutableViewModel
{
	private readonly IWalletModel _wallet;
	private readonly ScriptType _scriptType;

	[AutoNotify] private FlatTreeDataGridSource<AddressViewModel> _source = new(Enumerable.Empty<AddressViewModel>());

	private ReceiveAddressesViewModel(IWalletModel wallet, UnchainexWallet.Fluent.Models.Wallets.ScriptType scriptType)
	{
		_wallet = wallet;
		_scriptType = scriptType;

		EnableBack = true;
		SetupCancel(enableCancel: true, enableCancelOnEscape: true, enableCancelOnPressed: true);
	}

	public IEnumerable<SortableItem>? Sortables { get; private set; }

	protected override void OnNavigatedTo(bool isInHistory, CompositeDisposable disposables)
	{
		_wallet.Addresses.Unused
			.ToObservableChangeSet()
			.Filter(x => x.ScriptType == _scriptType)
			.Transform(CreateAddressViewModel)
			.DisposeMany()
			.Bind(out var unusedAddresses)
			.Subscribe()
			.DisposeWith(disposables);

		var source = ReceiveAddressesDataGridSource.Create(unusedAddresses);

		Source = source;
		Source.RowSelection!.SingleSelect = true;
		Source.DisposeWith(disposables);

		Sortables =
		[
			new SortableItem("Address") { SortByAscendingCommand = ReactiveCommand.Create(() => ((ITreeDataGridSource) Source).SortBy(Source.Columns[1], ListSortDirection.Ascending)), SortByDescendingCommand = ReactiveCommand.Create(() => ((ITreeDataGridSource) Source).SortBy(Source.Columns[1], ListSortDirection.Descending)) },
			new SortableItem("Label") { SortByAscendingCommand = ReactiveCommand.Create(() => ((ITreeDataGridSource) Source).SortBy(Source.Columns[2], ListSortDirection.Ascending)), SortByDescendingCommand = ReactiveCommand.Create(() => ((ITreeDataGridSource) Source).SortBy(Source.Columns[2], ListSortDirection.Descending)) }
		];

		base.OnNavigatedTo(isInHistory, disposables);
	}

	private AddressViewModel CreateAddressViewModel(IAddress address)
	{
		return new AddressViewModel(UiContext, OnEditAddressAsync, OnShowAddressAsync, address);
	}

	private void OnShowAddressAsync(IAddress a)
	{
		UiContext.Navigate().To().ReceiveAddress(_wallet, a, Services.UiConfig.Autocopy);
	}

	private async Task OnEditAddressAsync(IAddress address)
	{
		var result = await Navigate().To().AddressLabelEdit(_wallet, address).GetResultAsync();
		if (result is { } labels)
		{
			address.SetLabels(labels);
		}
	}
}
