using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Fluent.ViewModels.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Navigation;

public interface IWalletNavigation
{
	IWalletViewModel? To(IWalletModel wallet);
}

public interface IWalletSelector : IWalletNavigation
{
	IWalletViewModel? SelectedWallet { get; }

	IWalletModel? SelectedWalletModel { get; }
}
