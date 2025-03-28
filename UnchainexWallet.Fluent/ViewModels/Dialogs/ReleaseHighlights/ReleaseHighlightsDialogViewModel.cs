using ReactiveUI;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;
using UnchainexWallet.Helpers;
using Unit = System.Reactive.Unit;

namespace UnchainexWallet.Fluent.ViewModels.Dialogs.ReleaseHighlights;

[NavigationMetaData(NavigationTarget = NavigationTarget.DialogScreen, Title = "")]
public partial class ReleaseHighlightsDialogViewModel: DialogViewModelBase<Unit>
{
	public ReleaseHighlightsDialogViewModel(UiContext uiContext)
	{
		ReleaseHighlights = uiContext.ReleaseHighlights;

		NextCommand = ReactiveCommand.Create(() => Close());
		CancelCommand = ReactiveCommand.Create(() => Close(DialogResultKind.Cancel));

		SetupCancel(enableCancel: false, enableCancelOnEscape: true, enableCancelOnPressed: true);
	}

	public Announcements.ReleaseHighlights ReleaseHighlights { get; }
}
