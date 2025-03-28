using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Helpers;
using UnchainexWallet.Legal;

namespace UnchainexWallet.Backend.Controllers;

/// <summary>
/// To acquire Unchainex software related data.
/// </summary>
[Produces("application/json")]
[Route("api/v" + Constants.BackendMajorVersion + "/[controller]")]
public class UnchainexController : ControllerBase
{
	/// <summary>
	/// Gets the latest legal documents.
	/// </summary>
	/// <returns>Returns the legal documents.</returns>
	/// <response code="200">Returns the legal documents.</response>
	[HttpGet("legaldocuments")]
	[ProducesResponseType(typeof(byte[]), 200)]
	public async Task<IActionResult> GetLegalDocumentsAsync(string? id, CancellationToken cancellationToken)
	{
		string filePath;

		switch (id)
		{
			case "ww2":
				filePath = LegalDocuments.EmbeddedFilePathForWw2;
				break;

			case null:
				filePath = LegalDocuments.EmbeddedFilePathForWw1; // If the document id is null, then the request comes from WW 1.0 client.
				break;

			default:
				return NotFound();
		}

		var content = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);
		return File(content, "text/plain");
	}
}
