using NBitcoin;

namespace UnchainexWallet.Unchain.Models;

public record InputsRemovalRequest(
	uint256 RoundId,
	Guid AliceId
);
