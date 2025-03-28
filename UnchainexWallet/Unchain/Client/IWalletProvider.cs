using System.Collections.Generic;
using System.Threading.Tasks;
using UnchainexWallet.Wallets;

namespace UnchainexWallet.Unchain.Client;

public interface IWalletProvider
{
	Task<IEnumerable<IWallet>> GetWalletsAsync();
}
