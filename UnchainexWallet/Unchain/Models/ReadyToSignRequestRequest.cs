using NBitcoin;

namespace UnchainexWallet.Unchain.Models;

public record ReadyToSignRequestRequest(
	uint256 RoundId,
	Guid AliceId);
