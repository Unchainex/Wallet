using NBitcoin;
using System.Linq;
using UnchainexWallet.Blockchain.Analysis;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Tests.Helpers;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.BlockchainAnalysis;

/// <summary>
/// In these tests no inputs of a transaction are controlled by the user.
/// </summary>
public class ReceiveAnonScoreTests
{
	[Fact]
	public void NormalReceive()
	{
		var analyser = new BlockchainAnalyzer();
		var tx = BitcoinFactory.CreateSmartTransaction(1, 1, 0, 1);

		analyser.Analyze(tx);

		var coin = Assert.Single(tx.WalletOutputs);
		Assert.Equal(1, coin.HdPubKey.AnonymitySet);
	}

	[Fact]
	public void WholeCoinReceive()
	{
		var analyser = new BlockchainAnalyzer();
		var tx = BitcoinFactory.CreateSmartTransaction(1, 0, 0, 1);

		analyser.Analyze(tx);

		var coin = Assert.Single(tx.WalletOutputs);
		Assert.Equal(1, coin.HdPubKey.AnonymitySet);
	}

	[Fact]
	public void CoinjoinReceive()
	{
		var analyser = new BlockchainAnalyzer();
		var tx = BitcoinFactory.CreateSmartTransaction(10, Enumerable.Repeat(Money.Coins(1m), 9), Enumerable.Empty<(Money, int)>(), new[] { (Money.Coins(1m), HdPubKey.DefaultHighAnonymitySet) });

		analyser.Analyze(tx);

		var coin = Assert.Single(tx.WalletOutputs);
		Assert.Equal(1, coin.HdPubKey.AnonymitySet);
	}
}
