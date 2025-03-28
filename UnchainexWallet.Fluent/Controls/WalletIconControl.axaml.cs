using Avalonia;
using Avalonia.Controls.Primitives;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.Controls;

public class WalletIconControl : TemplatedControl
{
	public static readonly StyledProperty<WalletType> WalletTypeProperty = AvaloniaProperty.Register<WalletIconControl, WalletType>(nameof(WalletType));

	public WalletType WalletType
	{
		get => GetValue(WalletTypeProperty);
		set => SetValue(WalletTypeProperty, value);
	}
}
