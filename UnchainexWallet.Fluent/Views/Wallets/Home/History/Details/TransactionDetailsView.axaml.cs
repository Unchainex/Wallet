using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnchainexWallet.Fluent.Views.Wallets.Home.History.Details;

public class TransactionDetailsView : UserControl
{
	public TransactionDetailsView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
