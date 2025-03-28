using System.Threading.Tasks;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;

namespace UnchainexWallet.Fluent.ViewModels.Navigation;

public interface INavigate : IWalletNavigation
{
	INavigationStack<RoutableViewModel> HomeScreen { get; }

	INavigationStack<RoutableViewModel> DialogScreen { get; }

	INavigationStack<RoutableViewModel> FullScreen { get; }

	INavigationStack<RoutableViewModel> CompactDialogScreen { get; }

	IObservable<bool> IsDialogOpen { get; }

	bool IsAnyPageBusy { get; }

	INavigationStack<RoutableViewModel> Navigate(NavigationTarget target);

	FluentNavigate To();

	Task<DialogResult<TResult>> NavigateDialogAsync<TResult>(DialogViewModelBase<TResult> dialog, NavigationTarget target = NavigationTarget.Default, NavigationMode navigationMode = NavigationMode.Normal);
}
