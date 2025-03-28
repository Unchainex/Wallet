using System.Windows.Input;
using UnchainexWallet.Fluent.ViewModels.Navigation;

namespace UnchainexWallet.Fluent.ViewModels;

public abstract class TriggerCommandViewModel : RoutableViewModel
{
	public abstract ICommand TargetCommand { get; }
}
