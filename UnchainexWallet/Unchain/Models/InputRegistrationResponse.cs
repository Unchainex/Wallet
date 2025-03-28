using Unchain.CredentialRequesting;

namespace UnchainexWallet.Unchain.Models;

public record InputRegistrationResponse(
	Guid AliceId,
	CredentialsResponse AmountCredentials,
	CredentialsResponse VsizeCredentials
);
