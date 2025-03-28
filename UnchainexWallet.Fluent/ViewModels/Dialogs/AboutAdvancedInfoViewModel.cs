using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;
using UnchainexWallet.Helpers;
using UnchainexWallet.WebClients.Unchainex;

namespace UnchainexWallet.Fluent.ViewModels.Dialogs;

[NavigationMetaData(Title = "About", NavigationTarget = NavigationTarget.CompactDialogScreen)]
public partial class AboutAdvancedInfoViewModel : DialogViewModelBase<System.Reactive.Unit>
{
	public AboutAdvancedInfoViewModel()
	{
		SetupCancel(enableCancel: false, enableCancelOnEscape: true, enableCancelOnPressed: true);

		EnableBack = false;

		NextCommand = CancelCommand;
	}

	public Version BitcoinCoreVersion => Constants.BitcoinCoreVersion;

	public Version HwiVersion => Constants.HwiVersion;

	public string BackendCompatibleVersions => Constants.ClientSupportBackendVersionText;

	public string CurrentBackendMajorVersion => UnchainexClient.ApiVersion.ToString();

	protected override void OnDialogClosed()
	{
	}
}
