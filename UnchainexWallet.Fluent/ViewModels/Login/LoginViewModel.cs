using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.AddWallet;
using UnchainexWallet.Fluent.ViewModels.Navigation;
using UnchainexWallet.Userfacing;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Login;

[NavigationMetaData(Title = "")]
public partial class LoginViewModel : RoutableViewModel
{
	[AutoNotify] private string _password;
	[AutoNotify] private bool _isPasswordNeeded;
	[AutoNotify] private string _errorMessage;

	public LoginViewModel(IWalletModel wallet)
	{
		_password = "";
		_errorMessage = "";
		IsPasswordNeeded = !wallet.IsWatchOnlyWallet;
		WalletName = wallet.Name;
		WalletType = wallet.Settings.WalletType;

		NextCommand = ReactiveCommand.CreateFromTask(async () => await OnNextAsync(wallet));

		OkCommand = ReactiveCommand.Create(OnOk);

		EnableAutoBusyOn(NextCommand);
	}

	public WalletType WalletType { get; }

	public string WalletName { get; }

	public ICommand OkCommand { get; }

	private async Task OnNextAsync(IWalletModel walletModel)
	{
		var (success, compatibilityPasswordUsed) = await walletModel.Auth.TryLoginAsync(Password);

		if (!success)
		{
			ErrorMessage = "The passphrase is incorrect! Please try again.";
			return;
		}

		if (compatibilityPasswordUsed)
		{
			await ShowErrorAsync(Title, PasswordHelper.CompatibilityPasswordWarnMessage, "Compatibility password was used");
		}

		walletModel.Auth.CompleteLogin();
	}

	private void OnOk()
	{
		Password = "";
		ErrorMessage = "";
	}
}
