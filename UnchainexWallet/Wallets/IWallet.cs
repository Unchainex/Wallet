using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Blockchain.Transactions;
using UnchainexWallet.Models;
using UnchainexWallet.Unchain.Client;
using UnchainexWallet.Unchain.Client.Batching;

namespace UnchainexWallet.Wallets;

public interface IWallet
{
	string WalletName { get; }
	WalletId WalletId { get; }
	Money PlebStopThreshold { get; }
	bool IsMixable { get; }

	/// <summary>
	/// Watch only wallets have no key chains.
	/// </summary>
	IKeyChain? KeyChain { get; }

	IDestinationProvider DestinationProvider { get; }
	OutputProvider OutputProvider => new(DestinationProvider);
	PaymentBatch BatchedPayments => new();

	int AnonScoreTarget { get; }
	bool ConsolidationMode { get; set; }
	TimeSpan FeeRateMedianTimeFrame { get; }
	bool NonPrivateCoinIsolation { get; }

	Task<bool> IsWalletPrivateAsync();

	Task<IEnumerable<SmartCoin>> GetCoinjoinCoinCandidatesAsync();

	Task<IEnumerable<SmartTransaction>> GetTransactionsAsync();
}
