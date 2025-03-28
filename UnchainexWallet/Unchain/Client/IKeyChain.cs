using System.Collections.Generic;
using NBitcoin;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Crypto;

namespace UnchainexWallet.Unchain.Client;

public interface IKeyChain
{
	OwnershipProof GetOwnershipProof(IDestination destination, CoinJoinInputCommitmentData committedData);

	Transaction Sign(Transaction transaction, Coin coin, PrecomputedTransactionData precomputeTransactionData);
}
