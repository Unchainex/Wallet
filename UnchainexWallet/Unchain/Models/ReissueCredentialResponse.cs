using Unchain.CredentialRequesting;

namespace UnchainexWallet.Unchain.Models;

public record ReissueCredentialResponse(
	CredentialsResponse RealAmountCredentials,
	CredentialsResponse RealVsizeCredentials,
	CredentialsResponse ZeroAmountCredentials,
	CredentialsResponse ZeroVsizeCredentials
);
