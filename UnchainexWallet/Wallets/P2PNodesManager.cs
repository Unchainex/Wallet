using System.Threading;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Protocol;
using Unchain.Crypto.Randomness;
using UnchainexWallet.Extensions;
using UnchainexWallet.Helpers;
using UnchainexWallet.Logging;

namespace UnchainexWallet.Wallets;

public class P2PNodesManager
{
	public P2PNodesManager(Network network, NodesGroup nodes)
	{
		_network = network;
		_nodes = nodes;
	}

	private readonly Network _network;
	private readonly NodesGroup _nodes;
	private int NodeTimeouts { get; set; }
	public uint ConnectedNodesCount => (uint)_nodes.ConnectedNodes.Count;

	public async Task<Node?> GetNodeAsync(CancellationToken cancellationToken)
	{
		while (_nodes.ConnectedNodes.Count == 0)
		{
			await Task.Delay(100, cancellationToken).ConfigureAwait(false);
		}

		// Select a random node we are connected to.
		return _nodes.ConnectedNodes.RandomElement(SecureRandom.Instance);
	}

	public void DisconnectNodeIfEnoughPeers(Node node, string reason)
	{
		if (_nodes.ConnectedNodes.Count > 3)
		{
			DisconnectNode(node, reason);
		}
	}

	public void DisconnectNode(Node node, string reason)
	{
		Logger.LogInfo(reason);
		node.DisconnectAsync(reason);
	}

	public double GetCurrentTimeout()
	{
		// More permissive timeout if few nodes are connected to avoid exhaustion.
		return _nodes.ConnectedNodes.Count < 3
			? Math.Min(RuntimeParams.Instance.NetworkNodeTimeout * 1.5, 600)
			: RuntimeParams.Instance.NetworkNodeTimeout;
	}

	/// <summary>
	/// Current timeout used when downloading a block from the remote node. It is defined in seconds.
	/// </summary>
	public async Task UpdateTimeoutAsync(bool increaseDecrease)
	{
		if (increaseDecrease)
		{
			NodeTimeouts++;
		}
		else
		{
			NodeTimeouts--;
		}

		var timeout = RuntimeParams.Instance.NetworkNodeTimeout;

		// If it times out 2 times in a row then increase the timeout.
		if (NodeTimeouts >= 2)
		{
			NodeTimeouts = 0;
			timeout = (int)Math.Round(timeout * 1.5);
		}
		else if (NodeTimeouts <= -3) // If it does not time out 3 times in a row, lower the timeout.
		{
			NodeTimeouts = 0;
			timeout = (int)Math.Round(timeout * 0.7);
		}

		// Sanity check
		var minTimeout = _network == Network.Main ? 3 : 2;

		if (timeout < minTimeout)
		{
			timeout = minTimeout;
		}
		else if (timeout > 600)
		{
			timeout = 600;
		}

		if (timeout == RuntimeParams.Instance.NetworkNodeTimeout)
		{
			return;
		}

		RuntimeParams.Instance.NetworkNodeTimeout = timeout;
		await RuntimeParams.Instance.SaveAsync().ConfigureAwait(false);

		Logger.LogInfo($"Current timeout value used on block download is: {timeout} seconds.");
	}
}
