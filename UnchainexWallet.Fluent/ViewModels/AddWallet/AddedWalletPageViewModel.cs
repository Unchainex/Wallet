using ReactiveUI;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.Navigation;
using UnchainexWallet.Wallets;
using System.Reactive.Disposables;
using UnchainexWallet.Fluent.Models;
using System.Threading.Tasks;

namespace UnchainexWallet.Fluent.ViewModels.AddWallet;

[NavigationMetaData(Title = "Success")]
public partial class AddedWalletPageViewModel : RoutableViewModel
{
	private readonly IWalletSettingsModel _walletSettings;
	private IWalletModel? _wallet;

	private AddedWalletPageViewModel(IWalletSettingsModel walletSettings, WalletCreationOptions options)
	{
		_walletSettings = walletSettings;

		WalletName = options.WalletName!;
		WalletType = walletSettings.WalletType;

		SetupCancel(enableCancel: false, enableCancelOnEscape: false, enableCancelOnPressed: false);
		EnableBack = false;

		NextCommand = ReactiveCommand.CreateFromTask(() => OnNextAsync(options));
	}

	public WalletType WalletType { get; }

	public string WalletName { get; }

	private async Task OnNextAsync(WalletCreationOptions options)
	{
		if (_wallet is not { })
		{
			return;
		}

		IsBusy = true;

		await AutoLoginAsync(options);

		IsBusy = false;

		await Task.Delay(UiConstants.CloseSuccessDialogMillisecondsDelay);

		Navigate().Clear();

		UiContext.Navigate().To(_wallet);
	}

	protected override void OnNavigatedTo(bool isInHistory, CompositeDisposable disposables)
	{
		base.OnNavigatedTo(isInHistory, disposables);

		_wallet = UiContext.WalletRepository.SaveWallet(_walletSettings);

		if (NextCommand is not null && NextCommand.CanExecute(default))
		{
			NextCommand.Execute(default);
		}
	}

	private async Task AutoLoginAsync(WalletCreationOptions? options)
	{
		if (_wallet is not { })
		{
			return;
		}

		var password =
			options switch
			{
				WalletCreationOptions.AddNewWallet add => add.Password,
				WalletCreationOptions.RecoverWallet rec => rec.Password,
				WalletCreationOptions.ConnectToHardwareWallet => "",
				_ => null
			};

		if (password is { })
		{
			await _wallet.Auth.LoginAsync(password);
		}
	}
}
