using System.IO;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Helpers;

namespace UnchainexWallet.Tests.Helpers;

public static class LinuxStartupTestHelper
{
	public static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "autostart", "Unchainex.desktop");

	public static readonly string ExpectedDesktopFileContent = string.Join(
				"\n",
				"[Desktop Entry]",
				$"Name={Constants.AppName}",
				"Type=Application",
				$"Exec={EnvironmentHelpers.GetExecutablePath()} {StartupHelper.SilentArgument}",
				"Hidden=false",
				"Terminal=false",
				"X-GNOME-Autostart-enabled=true");

	public static string GetFileContent()
	{
		return string.Join("\n", File.ReadAllLines(FilePath));
	}
}
