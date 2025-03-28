using NBitcoin;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Blockchain.Transactions;

namespace UnchainexWallet.Tests.Helpers.AnalyzedTransaction;

public record WalletOutput(SmartCoin Coin)
{
	public double Anonymity => Coin.HdPubKey.AnonymitySet;

	public SmartCoin ToSmartCoin() => Coin;

	public ForeignOutput ToForeignOutput()
	{
		return new ForeignOutput(Coin.Transaction.Transaction, Coin.Index);
	}

	public static WalletOutput Create(Money amount, HdPubKey hdPubKey)
	{
		ForeignOutput output = ForeignOutput.Create(amount, hdPubKey.P2wpkhScript);
		SmartTransaction smartTransaction = new(output.Transaction, 0);
		SmartCoin smartCoin = new(smartTransaction, output.Index, hdPubKey);
		smartTransaction.TryAddWalletOutput(smartCoin);
		return new WalletOutput(smartCoin);
	}
}
