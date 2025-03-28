using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnchainexWallet.Fluent.Views.Wallets.Settings;

public class WalletCoinJoinSettingsView : UserControl
{
	public WalletCoinJoinSettingsView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
