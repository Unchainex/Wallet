using System.Collections.Generic;
using UnchainexWallet.Blockchain.TransactionOutputs;

namespace UnchainexWallet.Unchain.Client;

public class CoinRefrigerator
{
	private Dictionary<SmartCoin, DateTimeOffset> FrozenCoins { get; } = new();
	private readonly TimeSpan _freezeTime = TimeSpan.FromSeconds(90);

	public void Freeze(IEnumerable<SmartCoin> coins)
	{
		foreach (var coin in coins)
		{
			FrozenCoins[coin] = DateTimeOffset.UtcNow;
		}
	}

	public bool IsFrozen(SmartCoin coin)
	{
		if (!FrozenCoins.TryGetValue(coin, out var startTime))
		{
			return false;
		}

		if (startTime.Add(_freezeTime) > DateTimeOffset.UtcNow)
		{
			return true;
		}

		FrozenCoins.Remove(coin);
		return false;
	}
}
