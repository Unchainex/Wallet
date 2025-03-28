using System.Threading;
using System.Threading.Tasks;
using NBitcoin;
using UnchainexWallet.Blockchain.Keys;

namespace UnchainexWallet.WebClients.PayJoin;

public interface IPayjoinClient
{
	Uri PaymentUrl { get; }

	Task<PSBT> RequestPayjoin(PSBT originalTx, IHDKey accountKey, RootedKeyPath rootedKeyPath, HdPubKey changeHdPubKey, CancellationToken cancellationToken);
}
