using UnchainexWallet.Unchain.Backend.Rounds;

namespace UnchainexWallet.Unchain.Backend.Models;

public record WrongPhaseExceptionData(Phase CurrentPhase) : ExceptionData;
