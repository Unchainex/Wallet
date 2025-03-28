using System.Windows.Input;

namespace UnchainexWallet.Fluent.Controls;

public class UICommandDesign : IUICommand
{
	public string Name { get; set; } = null!;
	public object Icon { get; set; } = null!;
	public ICommand Command { get; set; } = null!;
	public bool IsDefault { get; set; }
}
