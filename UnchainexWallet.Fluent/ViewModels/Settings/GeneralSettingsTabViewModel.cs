using UnchainexWallet.Fluent.Infrastructure;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.ViewModels.Navigation;

namespace UnchainexWallet.Fluent.ViewModels.Settings;

[AppLifetime]
[NavigationMetaData(
	Title = "General",
	Caption = "Manage general settings",
	Order = 0,
	Category = "Settings",
	Keywords = new[]
	{
			"Settings", "General", "Dark", "Mode", "Run", "Computer", "System", "Start", "Background", "Close",
			"Auto", "Copy", "Paste", "Address", "Download", "New", "Version", "Enable", "GPU"
	},
	IconName = "settings_general_regular")]
public partial class GeneralSettingsTabViewModel : RoutableViewModel
{
	public GeneralSettingsTabViewModel(IApplicationSettings settings)
	{
		Settings = settings;
	}

	public bool IsReadOnly => Settings.IsOverridden;

	public IApplicationSettings Settings { get; }
}
