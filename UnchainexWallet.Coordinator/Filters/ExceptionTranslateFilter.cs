using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Unchain.Crypto;
using UnchainexWallet.Unchain;
using UnchainexWallet.Unchain.Backend.Models;
using UnchainexWallet.Unchain.Models;

namespace UnchainexWallet.Coordinator.Filters;

public class ExceptionTranslateAttribute : ExceptionFilterAttribute
{
	public override void OnException(ExceptionContext context)
	{
		var exception = context.Exception.InnerException ?? context.Exception;

		context.Result = exception switch
		{
			UnchainProtocolException e => new ObjectResult(new Error(
				Type: ProtocolConstants.ProtocolViolationType,
				ErrorCode: e.ErrorCode.ToString(),
				Description: e.Message,
				ExceptionData: e.ExceptionData ?? EmptyExceptionData.Instance))
			{
				StatusCode = (int)HttpStatusCode.InternalServerError
			},
			UnchainCryptoException e => new ObjectResult(new Error(
				Type: ProtocolConstants.ProtocolViolationType,
				ErrorCode: UnchainProtocolErrorCode.CryptoException.ToString(),
				Description: e.Message,
				ExceptionData: EmptyExceptionData.Instance))
			{
				StatusCode = (int)HttpStatusCode.InternalServerError
			},
			_ => new StatusCodeResult((int)HttpStatusCode.InternalServerError)
		};
	}
}
