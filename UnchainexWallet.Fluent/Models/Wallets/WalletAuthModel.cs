using System.Threading.Tasks;
using NBitcoin;
using ReactiveUI;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Userfacing;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Fluent.Models.Wallets;

[AutoInterface]
public partial class WalletAuthModel : ReactiveObject
{
	private readonly IWalletModel _walletModel;
	private readonly Wallet _wallet;
	[AutoNotify] private bool _isLoggedIn;

	public WalletAuthModel(IWalletModel walletModel, Wallet wallet)
	{
		_walletModel = walletModel;
		_wallet = wallet;
	}

	public bool HasPassword => !string.IsNullOrEmpty(_wallet.Password);

	public async Task LoginAsync(string password)
	{
		var isPasswordCorrect = await Task.Run(() => _wallet.TryLogin(password, out var _));
		if (!isPasswordCorrect)
		{
			throw new InvalidOperationException("Incorrect passphrase.");
		}

		CompleteLogin();
	}

	public async Task<WalletLoginResult> TryLoginAsync(string password)
	{
		string? compatibilityPassword = null;
		var isPasswordCorrect = await Task.Run(() => _wallet.TryLogin(password, out compatibilityPassword));

		var compatibilityPasswordUsed = compatibilityPassword is { };

		return new(isPasswordCorrect, compatibilityPasswordUsed);
	}

	public async Task<bool> TryPasswordAsync(string password)
	{
		return await Task.Run(() => PasswordHelper.TryPassword(_wallet.KeyManager, password, out _));
	}

	public void CompleteLogin()
	{
		IsLoggedIn = true;
	}

	public void Logout()
	{
		_wallet.Logout();
		IsLoggedIn = false;
	}

	public bool VerifyRecoveryWords(Mnemonic mnemonic)
	{
		var saltSoup = _wallet.Password;

		var recovered = KeyManager.Recover(
			mnemonic,
			saltSoup,
			_wallet.Network,
			_wallet.KeyManager.SegwitAccountKeyPath,
			null,
			null,
			_wallet.KeyManager.MinGapLimit);

		var result = _wallet.KeyManager.SegwitExtPubKey == recovered.SegwitExtPubKey;

		return result;
	}
}
