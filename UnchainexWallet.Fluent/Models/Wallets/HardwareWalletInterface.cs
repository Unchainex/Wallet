using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Helpers;
using UnchainexWallet.Hwi.Models;

namespace UnchainexWallet.Fluent.Models.Wallets;

[AutoInterface]
public partial class HardwareWalletInterface
{
	public Task<HwiEnumerateEntry[]> DetectAsync(CancellationToken cancelToken)
	{
		return HardwareWalletOperationHelpers.DetectAsync(Services.WalletManager.Network, cancelToken);
	}

	public Task InitHardwareWalletAsync(HwiEnumerateEntry device, CancellationToken cancelToken)
	{
		return HardwareWalletOperationHelpers.InitHardwareWalletAsync(device, Services.WalletManager.Network, cancelToken);
	}
}
