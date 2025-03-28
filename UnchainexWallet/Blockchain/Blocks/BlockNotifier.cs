using NBitcoin;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Bases;
using UnchainexWallet.BitcoinRpc;
using UnchainexWallet.Extensions;
using UnchainexWallet.Helpers;

namespace UnchainexWallet.Blockchain.Blocks;

public class BlockNotifier : PeriodicRunner
{
	public BlockNotifier(IRPCClient rpcClient, TimeSpan? period = null) : base(period ?? TimeSpan.FromSeconds(7))
	{
		RpcClient = Guard.NotNull(nameof(rpcClient), rpcClient);
		_processedBlocks = new List<uint256>();
	}

	public event EventHandler<Block>? OnBlock;

	public event EventHandler<uint256>? OnReorg;

	public IRPCClient RpcClient { get; set; }
	public Network Network => RpcClient.Network;

	private readonly List<uint256> _processedBlocks;

	public uint256 BestBlockHash { get; private set; } = uint256.Zero;

	private uint256? LastInv { get; set; } = null;
	private readonly object _lastInvLock = new();

	private void P2pNode_BlockInv(object? sender, uint256 blockHash)
	{
		lock (_lastInvLock)
		{
			LastInv = blockHash;
		}
		TriggerRound();
	}

	protected override async Task ActionAsync(CancellationToken cancel)
	{
		uint256 bestBlockHash;
		uint256? lastInv;
		lock (_lastInvLock)
		{
			lastInv = LastInv;
		}

		// If we did not yet process our last inv, then we can take this as the best known block hash, so we don't need the RPC command.
		// Otherwise make the RPC command.
		if (lastInv is { } && !_processedBlocks.Contains(lastInv))
		{
			bestBlockHash = lastInv;
		}
		else
		{
			bestBlockHash = await RpcClient.GetBestBlockHashAsync(cancel).ConfigureAwait(false);
		}

		// If there's no new block.
		if (bestBlockHash == BestBlockHash)
		{
			return;
		}

		var arrivedBlock = await RpcClient.GetBlockAsync(bestBlockHash, cancel).ConfigureAwait(false);
		var arrivedHeader = arrivedBlock.Header;
		arrivedHeader.PrecomputeHash(false, true);

		// If we haven't processed any block yet then we're processing the first seven to avoid accidental reorgs.
		// 7 blocks, because
		//   - That was the largest recorded reorg so far.
		//   - Reorg in this point of time would be very unlikely anyway.
		//   - 100 blocks would be the sure, but that'd be a huge performance overkill.
		if (_processedBlocks.Count == 0)
		{
			var reorgProtection7Headers = new List<BlockHeader>()
				{
					arrivedHeader
				};

			var currentHeader = arrivedHeader;
			while (reorgProtection7Headers.Count < 7 && currentHeader.GetHash() != Network.GenesisHash)
			{
				currentHeader = await RpcClient.GetBlockHeaderAsync(currentHeader.HashPrevBlock, cancel).ConfigureAwait(false);
				reorgProtection7Headers.Add(currentHeader);
			}

			reorgProtection7Headers.Reverse();
			foreach (var header in reorgProtection7Headers)
			{
				// It's initialization. Don't notify about it.
				AddHeader(header);
			}

			BestBlockHash = bestBlockHash;
			return;
		}

		// If block was already processed return.
		if (_processedBlocks.Contains(arrivedHeader.GetHash()))
		{
			BestBlockHash = bestBlockHash;
			return;
		}

		// If this block follows the proper order then add.
		if (_processedBlocks.Last() == arrivedHeader.HashPrevBlock)
		{
			AddBlock(arrivedBlock);
			BestBlockHash = bestBlockHash;
			return;
		}

		// Else let's sort out things.
		var foundPrevBlock = _processedBlocks.FirstOrDefault(x => x == arrivedHeader.HashPrevBlock);

		// Missed notifications on some previous blocks.
		if (foundPrevBlock is { })
		{
			// Reorg happened.
			ReorgToBlock(foundPrevBlock);
			AddBlock(arrivedBlock);
			BestBlockHash = bestBlockHash;
			return;
		}

		await HandleMissedBlocksAsync(arrivedBlock, cancel).ConfigureAwait(false);

		BestBlockHash = bestBlockHash;
		return;
	}

	private async Task HandleMissedBlocksAsync(Block arrivedBlock, CancellationToken cancellationToken)
	{
		List<Block> missedBlocks = new()
		{
			arrivedBlock
		};
		var currentHeader = arrivedBlock.Header;
		while (true)
		{
			Block missedBlock = await RpcClient.GetBlockAsync(currentHeader.HashPrevBlock, cancellationToken).ConfigureAwait(false);

			if (missedBlocks.Count > 144)
			{
				missedBlocks.RemoveFirst();
			}

			currentHeader = missedBlock.Header;
			currentHeader.PrecomputeHash(false, true);
			missedBlocks.Add(missedBlock);

			if (currentHeader.GetHash() == Network.GenesisHash)
			{
				var processedBlocksClone = _processedBlocks.ToArray();
				var processedReversedBlocks = processedBlocksClone.Reverse();
				_processedBlocks.Clear();
				foreach (var processedBlock in processedReversedBlocks)
				{
					OnReorg?.Invoke(this, processedBlock);
				}
				break;
			}

			// If we found the proper chain.
			var foundPrevBlock = _processedBlocks.FirstOrDefault(x => x == currentHeader.HashPrevBlock);
			if (foundPrevBlock is { })
			{
				// If the last block hash is not what we found, then we missed a reorg also.
				if (foundPrevBlock != _processedBlocks.Last())
				{
					ReorgToBlock(foundPrevBlock);
				}

				break;
			}
		}

		missedBlocks.Reverse();
		foreach (var b in missedBlocks)
		{
			AddBlock(b);
		}
	}

	private void AddBlock(Block block)
	{
		AddHeader(block.Header);
		OnBlock?.Invoke(this, block);
	}

	private void AddHeader(BlockHeader block)
	{
		_processedBlocks.Add(block.GetHash());
	}

	private void ReorgToBlock(uint256 correctBlock)
	{
		var index = _processedBlocks.IndexOf(correctBlock);
		int countToRemove = _processedBlocks.Count - (index + 1);
		var toRemoves = _processedBlocks.TakeLast(countToRemove).ToList();
		_processedBlocks.RemoveRange(index + 1, countToRemove);
		toRemoves.Reverse();
		foreach (var toRemove in toRemoves)
		{
			OnReorg?.Invoke(this, toRemove);
		}
	}
}
