using NBitcoin;
using Unchain.CredentialRequesting;
using UnchainexWallet.Crypto;

namespace UnchainexWallet.Unchain.Models;

public record InputRegistrationRequest(
	uint256 RoundId,
	OutPoint Input,
	OwnershipProof OwnershipProof,
	ZeroCredentialsRequest ZeroAmountCredentialRequests,
	ZeroCredentialsRequest ZeroVsizeCredentialRequests
);
