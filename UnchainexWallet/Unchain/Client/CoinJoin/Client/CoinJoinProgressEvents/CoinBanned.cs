using UnchainexWallet.Blockchain.TransactionOutputs;

namespace UnchainexWallet.Unchain.Client.CoinJoinProgressEvents;

public class CoinBanned : CoinJoinProgressEventArgs
{
	public CoinBanned(SmartCoin coin, DateTimeOffset banUntilUtc)
	{
		Coin = coin;
		BanUntilUtc = banUntilUtc;
	}

	public SmartCoin Coin { get; }
	public DateTimeOffset BanUntilUtc { get; }
}
