using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;
using UnchainexWallet.BitcoinRpc;
using UnchainexWallet.Helpers;
using UnchainexWallet.Logging;


namespace UnchainexWallet.Coordinator;

public class StartupTask
{
	private IRPCClient RpcClient { get; }

	public StartupTask(IRPCClient rpc)
	{
		RpcClient = rpc;
	}

	public async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		Logger.LogInfo("Unchainex Coordinator");

		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

		// Make sure RPC works.
		await AssertRpcNodeFullyInitializedAsync(cancellationToken).ConfigureAwait(false);
	}

	private async Task AssertRpcNodeFullyInitializedAsync(CancellationToken cancellationToken)
	{
		try
		{
			var blockchainInfo = await RpcClient.GetBlockchainInfoAsync(cancellationToken);

			var blocks = blockchainInfo.Blocks;
			if (blocks == 0 && RpcClient.Network != Network.RegTest)
			{
				throw new NotSupportedException($"{nameof(blocks)} == 0");
			}

			var headers = blockchainInfo.Headers;
			if (headers == 0 && RpcClient.Network != Network.RegTest)
			{
				throw new NotSupportedException($"{nameof(headers)} == 0");
			}

			if (blocks != headers)
			{
				throw new NotSupportedException($"Bitcoin Node is not fully synchronized.");
			}

			Logger.LogInfo($"Bitcoin Node is fully synchronized.");

			if (RpcClient.Network == Network.RegTest) // Make sure there's at least 101 block, if not generate it
			{
				if (blocks < 101)
				{
					using Key key = new();
					var generateBlocksResponse = await RpcClient.GenerateToAddressAsync(101, key.GetAddress(ScriptPubKeyType.Segwit, Network.RegTest), cancellationToken);
					if (generateBlocksResponse is null)
					{
						throw new NotSupportedException($"Bitcoin Node cannot generate blocks on the {Network.RegTest}.");
					}

					blockchainInfo = await RpcClient.GetBlockchainInfoAsync(cancellationToken);
					blocks = blockchainInfo.Blocks;
					if (blocks == 0)
					{
						throw new NotSupportedException($"{nameof(blocks)} == 0");
					}
					Logger.LogInfo($"Generated 101 block on {Network.RegTest}. Number of blocks {blocks}.");
				}
			}
		}
		catch (WebException)
		{
			Logger.LogError($"Bitcoin Node is not running, or incorrect RPC credentials, or network is given in the config file.");
			throw;
		}
	}

	private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		Logger.LogWarning(e.Exception, "Unexpected unobserved task exception.");
	}

	private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			Logger.LogWarning(ex, "Unhandled exception.");
		}
	}
}
