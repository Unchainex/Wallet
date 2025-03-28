using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Blockchain.TransactionProcessing;

namespace UnchainexWallet.Tests.Helpers;

public static class TransactionProcessorExtensions
{
	public static HdPubKey NewKey(this TransactionProcessor me, string label)
	{
		return me.KeyManager.GenerateNewKey(label, KeyState.Clean, isInternal: true);
	}
}
