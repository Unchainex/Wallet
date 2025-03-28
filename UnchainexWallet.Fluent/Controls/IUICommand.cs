using System.Windows.Input;

namespace UnchainexWallet.Fluent.Controls;

public interface IUICommand
{
	public string Name { get; }
	public object Icon { get; }
	public ICommand Command { get; }
	public bool IsDefault { get; }
}
