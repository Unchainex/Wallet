using System.Collections.Immutable;
using NBitcoin;
using Unchain.Crypto;
using Unchain.Crypto.StrobeProtocol;
using UnchainexWallet.Unchain.Models;
using UnchainexWallet.Crypto.StrobeProtocol;

namespace UnchainexWallet.Unchain.Crypto;

public static class RoundHasher
{
	public static uint256 CalculateHash(
			DateTimeOffset inputRegistrationStart,
			TimeSpan inputRegistrationTimeout,
			TimeSpan connectionConfirmationTimeout,
			TimeSpan outputRegistrationTimeout,
			TimeSpan transactionSigningTimeout,
			MoneyRange allowedInputAmounts,
			ImmutableSortedSet<ScriptType> allowedInputTypes,
			MoneyRange allowedOutputAmounts,
			ImmutableSortedSet<ScriptType> allowedOutputTypes,
			Network network,
			long feePerK,
			int maxTransactionSize,
			long minRelayTxFeePerK,
			long maxAmountCredentialValue,
			long maxVsizeCredentialValue,
			long maxVsizeAllocationPerAlice,
			long maxSuggestedAmount,
			string coordinationIdentifier,
			CredentialIssuerParameters amountCredentialIssuerParameters,
			CredentialIssuerParameters vsizeCredentialIssuerParameters)
	{
		var hash = StrobeHasher.Create(ProtocolConstants.RoundStrobeDomain)
			.Append(ProtocolConstants.RoundInputRegistrationStartStrobeLabel, inputRegistrationStart)
			.Append(ProtocolConstants.RoundInputRegistrationTimeoutStrobeLabel, inputRegistrationTimeout)
			.Append(ProtocolConstants.RoundConnectionConfirmationTimeoutStrobeLabel, connectionConfirmationTimeout)
			.Append(ProtocolConstants.RoundOutputRegistrationTimeoutStrobeLabel, outputRegistrationTimeout)
			.Append(ProtocolConstants.RoundTransactionSigningTimeoutStrobeLabel, transactionSigningTimeout)
			.Append(ProtocolConstants.RoundAllowedInputAmountsStrobeLabel, allowedInputAmounts)
			.Append(ProtocolConstants.RoundAllowedInputTypesStrobeLabel, allowedInputTypes)
			.Append(ProtocolConstants.RoundAllowedOutputAmountsStrobeLabel, allowedOutputAmounts)
			.Append(ProtocolConstants.RoundAllowedOutputTypesStrobeLabel, allowedOutputTypes)
			.Append(ProtocolConstants.RoundNetworkStrobeLabel, network.ToString())
			.Append(ProtocolConstants.RoundFeeRateStrobeLabel, feePerK)
			.AppendDummyCoordinationFee(ProtocolConstants.RoundCoordinationFeeRateStrobeLabel)
			.Append(ProtocolConstants.RoundMaxTransactionSizeStrobeLabel, maxTransactionSize)
			.Append(ProtocolConstants.RoundMinRelayTxFeeStrobeLabel, minRelayTxFeePerK)
			.Append(ProtocolConstants.RoundMaxAmountCredentialValueStrobeLabel, maxAmountCredentialValue)
			.Append(ProtocolConstants.RoundMaxVsizeCredentialValueStrobeLabel, maxVsizeCredentialValue)
			.Append(ProtocolConstants.RoundMaxVsizePerAliceStrobeLabel, maxVsizeAllocationPerAlice)
			.Append(ProtocolConstants.RoundMaxSuggestedAmountLabel, maxSuggestedAmount)
			.Append(ProtocolConstants.RoundCoordinationIdentifier, coordinationIdentifier)
			.Append(ProtocolConstants.RoundAmountCredentialIssuerParametersStrobeLabel, amountCredentialIssuerParameters)
			.Append(ProtocolConstants.RoundVsizeCredentialIssuerParametersStrobeLabel, vsizeCredentialIssuerParameters)
			.GetHash();
		return new uint256(hash);
	}
}
