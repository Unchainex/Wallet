using System.Threading.Tasks;

namespace UnchainexWallet.Fluent.Models.Wallets;

public interface IHardwareWalletModel : IWalletModel
{
	Task<bool> AuthorizeTransactionAsync(TransactionAuthorizationInfo transactionAuthorizationInfo);
}
