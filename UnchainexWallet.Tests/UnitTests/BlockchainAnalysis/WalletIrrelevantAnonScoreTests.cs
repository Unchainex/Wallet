using UnchainexWallet.Blockchain.Analysis;
using UnchainexWallet.Tests.Helpers;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.BlockchainAnalysis;

/// <summary>
/// In these tests no inputs, nor outputs in a transaction are controlled by the user.
/// </summary>
public class WalletIrrelevantAnonScoreTests
{
	[Fact]
	public void OneInOneOut()
	{
		var analyser = new BlockchainAnalyzer();
		var tx = BitcoinFactory.CreateSmartTransaction(1, 1, 0, 0);

		analyser.Analyze(tx);

		Assert.Empty(tx.WalletInputs);
		Assert.Empty(tx.WalletOutputs);
	}

	[Fact]
	public void ManyInManyOut()
	{
		var analyser = new BlockchainAnalyzer();
		var tx = BitcoinFactory.CreateSmartTransaction(3, 3, 0, 0);

		analyser.Analyze(tx);

		Assert.Empty(tx.WalletInputs);
		Assert.Empty(tx.WalletOutputs);
	}
}
