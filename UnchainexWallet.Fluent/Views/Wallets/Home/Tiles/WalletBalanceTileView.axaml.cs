using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnchainexWallet.Fluent.Views.Wallets.Home.Tiles;

public class WalletBalanceTileView : UserControl
{
	public WalletBalanceTileView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
