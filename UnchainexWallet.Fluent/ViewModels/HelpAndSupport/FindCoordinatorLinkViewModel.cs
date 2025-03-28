using System.Windows.Input;
using ReactiveUI;
using UnchainexWallet.Fluent.ViewModels.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.HelpAndSupport;

[NavigationMetaData(
	Title = "Find a Coordinator",
	Caption = "Open Unchainex's documentation website",
	Order = 3,
	Category = "Help & Support",
	Keywords =
	[
		"Find", "Coordinator", "Coinjoin", "Docs", "Documentation", "Guide"
	],
	IconName = "book_question_mark_regular")]
public partial class FindCoordinatorLinkViewModel : TriggerCommandViewModel
{
	private FindCoordinatorLinkViewModel()
	{
		TargetCommand = ReactiveCommand.CreateFromTask(async () => await UiContext.FileSystem.OpenBrowserAsync(WalletViewModel.FindCoordinatorLink));
	}

	public override ICommand TargetCommand { get; }
}
