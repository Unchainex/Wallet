using UnchainexWallet.Fluent.Models.Wallets;

namespace UnchainexWallet.Fluent.ViewModels.Wallets.Home.Tiles;

public class WalletBalanceTileViewModel : ActivatableViewModel
{
	public WalletBalanceTileViewModel(IObservable<Amount> amounts)
	{
		Amounts = amounts;
	}

	public IObservable<Amount> Amounts { get; }
}
