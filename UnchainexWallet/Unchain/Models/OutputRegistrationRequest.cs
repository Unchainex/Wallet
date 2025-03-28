using NBitcoin;
using Unchain.CredentialRequesting;

namespace UnchainexWallet.Unchain.Models;

public record OutputRegistrationRequest(
	uint256 RoundId,
	Script Script,
	RealCredentialsRequest AmountCredentialRequests,
	RealCredentialsRequest VsizeCredentialRequests
);
