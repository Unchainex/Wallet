using ReactiveUI;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;

namespace UnchainexWallet.Fluent.ViewModels.Dialogs;

[NavigationMetaData(Title = "Hide Address", NavigationTarget = NavigationTarget.CompactDialogScreen)]
public partial class ConfirmHideAddressViewModel : DialogViewModelBase<bool>
{
	public ConfirmHideAddressViewModel(LabelsArray labels)
	{
		Labels = labels;

		NextCommand = ReactiveCommand.Create(() => Close(result: true));
		CancelCommand = ReactiveCommand.Create(() => Close(DialogResultKind.Cancel));

		SetupCancel(enableCancel: false, enableCancelOnEscape: true, enableCancelOnPressed: true);
	}

	public LabelsArray Labels { get; }
}
