using Avalonia.Controls;
using UnchainexWallet.Fluent.Models.Wallets;

namespace UnchainexWallet.Tests.UnitTests.Fluent.Controls.AmountControlTests;

public partial class AmountControl_Defaults : Window
{
    public AmountControl_Defaults()
    {
        InitializeComponent();

        DataContext = Amount.Zero;
    }
}
