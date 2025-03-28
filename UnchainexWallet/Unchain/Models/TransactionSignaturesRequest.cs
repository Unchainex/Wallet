using NBitcoin;

namespace UnchainexWallet.Unchain.Models;

public record TransactionSignaturesRequest(uint256 RoundId, uint InputIndex, WitScript Witness);
