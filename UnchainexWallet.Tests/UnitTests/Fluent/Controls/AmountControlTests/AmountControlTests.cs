using Avalonia.Headless.XUnit;
using UnchainexWallet.Fluent.Models.Wallets;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Fluent.Controls.AmountControlTests;

public class AmountControlTests
{
    //[AvaloniaFact]
    public void AmountControl_Defaults()
    {
        var window = new AmountControl_Defaults();
        window.Show();

        Assert.Equal(window.DataContext, Amount.Zero);
    }
}
