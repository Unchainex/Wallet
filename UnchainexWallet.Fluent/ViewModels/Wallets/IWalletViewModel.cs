using NBitcoin;

namespace UnchainexWallet.Fluent.ViewModels.Wallets;

public interface IWalletViewModel
{
	void SelectTransaction(uint256 txid);
}
