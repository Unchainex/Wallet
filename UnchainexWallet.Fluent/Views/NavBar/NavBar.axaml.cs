using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnchainexWallet.Fluent.Views.NavBar;

public class NavBar : UserControl
{
	public NavBar()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
