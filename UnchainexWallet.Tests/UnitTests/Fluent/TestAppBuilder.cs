using Avalonia;
using Avalonia.Headless;
using UnchainexWallet.Tests.UnitTests.Fluent;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace UnchainexWallet.Tests.UnitTests.Fluent;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}
