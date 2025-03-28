namespace UnchainexWallet.Unchain.Backend.Models;

public record InputBannedExceptionData(DateTimeOffset BannedUntil) : ExceptionData;
