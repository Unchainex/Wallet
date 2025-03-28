using NBitcoin;
using System.Linq;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Client;
using UnchainexWallet.Wallets;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Client;

public class KeyChainTests
{
	[Fact]
	public void SignTransactionTest()
	{
		var keyManager = KeyManager.CreateNew(out _, "", Network.Main);
		var destinationProvider = new InternalDestinationProvider(keyManager);
		var keyChain = new KeyChain(keyManager,"");

		var coinDestination = destinationProvider.GetNextDestinations(1, false).First();
		var coin = new Coin(BitcoinFactory.CreateOutPoint(), new TxOut(Money.Coins(1.0m), coinDestination));

		var transaction = Transaction.Create(Network.Main); // the transaction doesn't contain the input that we request to be signed.

		Assert.Throws<ArgumentException>(() => keyChain.Sign(transaction, coin, transaction.PrecomputeTransactionData()));

		transaction.Inputs.Add(coin.Outpoint);
		var signedTx = keyChain.Sign(transaction, coin, transaction.PrecomputeTransactionData(new[] { coin }));
		Assert.True(signedTx.HasWitness);
	}
}
