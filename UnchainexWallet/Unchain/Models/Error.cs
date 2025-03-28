using UnchainexWallet.Unchain.Backend.Models;

namespace UnchainexWallet.Unchain.Models;

public record Error(
	string Type,
	string ErrorCode,
	string Description,
	ExceptionData ExceptionData
);
