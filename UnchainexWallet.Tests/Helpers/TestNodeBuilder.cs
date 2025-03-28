using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Blockchain.Mempool;
using UnchainexWallet.Helpers;
using UnchainexWallet.Tests.BitcoinCore;
using UnchainexWallet.Tests.BitcoinCore.Endpointing;

namespace UnchainexWallet.Tests.Helpers;

public static class TestNodeBuilder
{
	public static async Task<CoreNode> CreateAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", string additionalFolder = "", MempoolService? mempoolService = null)
	{
		var dataDir = Path.Combine(Common.GetWorkDir(callerFilePath, callerMemberName), additionalFolder);

		CoreNodeParams nodeParameters = CreateDefaultCoreNodeParams(mempoolService ?? new MempoolService(), dataDir);
		return await CoreNode.CreateAsync(
			nodeParameters,
			CancellationToken.None);
	}

	public static async Task<CoreNode> CreateForHeavyConcurrencyAsync([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", string additionalFolder = "", MempoolService? mempoolService = null)
	{
		var dataDir = Path.Combine(Common.GetWorkDir(callerFilePath, callerMemberName), additionalFolder);

		CoreNodeParams nodeParameters = CreateDefaultCoreNodeParams(mempoolService ?? new MempoolService(), dataDir);
		nodeParameters.RpcWorkQueue = 32;
		return await CoreNode.CreateAsync(
			nodeParameters,
			CancellationToken.None);
	}

	private static CoreNodeParams CreateDefaultCoreNodeParams(MempoolService mempoolService, string dataDir)
	{
		var nodeParameters = new CoreNodeParams(
				Network.RegTest,
				mempoolService ?? new MempoolService(),
				dataDir,
				tryRestart: true,
				tryDeleteDataDir: true,
				EndPointStrategy.Random,
				EndPointStrategy.Random,
				txIndex: 1,
				prune: 0,
				disableWallet: 0,
				mempoolReplacement: "fee,optin",
				userAgent: $"/UnchainexClient:{Constants.ClientVersion}/",
				fallbackFee: Money.Coins(0.0002m), // https://github.com/bitcoin/bitcoin/pull/16524
				new MemoryCache(new MemoryCacheOptions()));
		nodeParameters.ListenOnion = 0;
		nodeParameters.Discover = 0;
		nodeParameters.DnsSeed = 0;
		nodeParameters.FixedSeeds = 0;
		nodeParameters.Upnp = 0;
		nodeParameters.NatPmp = 0;
		nodeParameters.PersistMempool = 0;
		return nodeParameters;
	}
}
