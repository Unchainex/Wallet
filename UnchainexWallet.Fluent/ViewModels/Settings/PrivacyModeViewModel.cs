using System.Reactive.Linq;
using ReactiveUI;
using UnchainexWallet.Fluent.Infrastructure;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.ViewModels.Navigation;

namespace UnchainexWallet.Fluent.ViewModels.Settings;

[AppLifetime]
[NavigationMetaData(
	Title = "Discreet Mode",
	Searchable = false,
	NavBarPosition = NavBarPosition.Bottom,
	NavBarSelectionMode = NavBarSelectionMode.Toggle)]
public partial class PrivacyModeViewModel : RoutableViewModel
{
	[AutoNotify] private bool _privacyMode;
	[AutoNotify] private string? _iconName;
	[AutoNotify] private string? _iconNameFocused;

	public PrivacyModeViewModel(IApplicationSettings applicationSettings)
	{
		_privacyMode = applicationSettings.PrivacyMode;

		SetIcon();

		this.WhenAnyValue(x => x.PrivacyMode)
			.Skip(1)
			.Do(x => applicationSettings.PrivacyMode = x)
			.Subscribe();
	}

	public void Toggle()
	{
		PrivacyMode = !PrivacyMode;
		SetIcon();
	}

	public void SetIcon()
	{
		IconName = PrivacyMode ? "eye_hide_regular" : "eye_show_regular";
	}
}
