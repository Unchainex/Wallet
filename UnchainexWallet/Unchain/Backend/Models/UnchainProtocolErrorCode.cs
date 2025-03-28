namespace UnchainexWallet.Unchain.Backend.Models;

public enum UnchainProtocolErrorCode
{
	RoundNotFound,
	WrongPhase,
	InputSpent,
	InputUnconfirmed,
	InputImmature,
	WrongOwnershipProof,
	TooManyInputs,
	NotEnoughFunds,
	TooMuchFunds,
	NonUniqueInputs,
	InputBanned,
	InputLongBanned,
	InputNotWhitelisted,
	AliceNotFound,
	IncorrectRequestedVsizeCredentials,
	TooMuchVsize,
	ScriptNotAllowed,
	IncorrectRequestedAmountCredentials,
	WrongCoinjoinSignature,
	AliceAlreadyRegistered,
	NonStandardInput,
	NonStandardOutput,
	WitnessAlreadyProvided,
	InsufficientFees,
	SizeLimitExceeded,
	DustOutput,
	UneconomicalInput,
	VsizeQuotaExceeded,
	DeltaNotZero,
	WrongNumberOfCreds,
	CryptoException,
	AliceAlreadySignalled,
	AliceAlreadyConfirmedConnection,
	AlreadyRegisteredScript,
	SignatureTooLong,
}

public static class UnchainProtocolErrorCodeExtension
{
	public static bool IsEvidencingClearMisbehavior(this UnchainProtocolErrorCode errorCode) =>
		errorCode
			is UnchainProtocolErrorCode.ScriptNotAllowed
			or UnchainProtocolErrorCode.NonStandardInput
			or UnchainProtocolErrorCode.NonStandardOutput
			or UnchainProtocolErrorCode.DeltaNotZero
			or UnchainProtocolErrorCode.WrongNumberOfCreds
			or UnchainProtocolErrorCode.NonUniqueInputs
			or UnchainProtocolErrorCode.CryptoException
			or UnchainProtocolErrorCode.AliceAlreadyConfirmedConnection;
}
