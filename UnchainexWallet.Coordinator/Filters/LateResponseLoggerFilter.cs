using Microsoft.AspNetCore.Mvc.Filters;
using UnchainexWallet.Logging;
using UnchainexWallet.Unchain.Backend.Models;

namespace UnchainexWallet.Coordinator.Filters;

public class LateResponseLoggerFilter : ExceptionFilterAttribute
{
	public override void OnException(ExceptionContext context)
	{
		if (context.Exception is not WrongPhaseException ex)
		{
			return;
		}

		var actionName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ActionName;

		Logger.LogInfo($"Request '{actionName}' missing the phase '{string.Join(",", ex.ExpectedPhases)}' ('{ex.PhaseTimeout}' timeout) by '{ex.Late}'. Round id '{ex.RoundId}'.");
	}
}
