using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnchainexWallet.Fluent.Views.AddWallet;

public class AddWalletPageView : UserControl
{
	public AddWalletPageView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
