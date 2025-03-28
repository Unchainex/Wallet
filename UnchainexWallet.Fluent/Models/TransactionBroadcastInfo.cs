using UnchainexWallet.Fluent.Models.Wallets;

namespace UnchainexWallet.Fluent.Models;

public record TransactionBroadcastInfo(string TransactionId, int InputCount, int OutputCount, Amount? InputAmount, Amount? OutputAmount, Amount? NetworkFee);
