using System.Linq;

namespace UnchainexWallet.Unchain.Backend.Models;

public class UnchainProtocolException : Exception
{
	public UnchainProtocolException(UnchainProtocolErrorCode errorCode, string? message = null, Exception? innerException = null, ExceptionData? exceptionData = null)
		: base(message ?? ErrorCodeDescription(errorCode), innerException)
	{
		ErrorCode = errorCode;
		ExceptionData = exceptionData;
	}

	public UnchainProtocolErrorCode ErrorCode { get; }
	public ExceptionData? ExceptionData { get; }

	private static string ErrorCodeDescription(UnchainProtocolErrorCode errorCode)
	{
		var enumName = Enum.GetName(errorCode) ?? "";
		var errorDescription = string.Join(
			"",
			enumName.Select((c, i) => i > 0 && char.IsUpper(c)
				? " " + char.ToLowerInvariant(c)
				: "" + c));
		return errorDescription;
	}
}
