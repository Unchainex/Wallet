using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBitcoin.RPC;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Backend.Models;
using UnchainexWallet.Backend.Models.Responses;
using UnchainexWallet.Blockchain.BlockFilters;
using UnchainexWallet.Helpers;
using UnchainexWallet.Logging;
using UnchainexWallet.Models;

namespace UnchainexWallet.Backend.Controllers;

/// <summary>
/// To make batched requests.
/// </summary>
[Produces("application/json")]
[Route("api/v" + Constants.BackendMajorVersion + "/btc/[controller]")]
public class BatchController : ControllerBase
{
	public BatchController(BlockchainController blockchainController, OffchainController offchainController, IndexBuilderService indexBuilderService, Config config)
	{
		BlockchainController = blockchainController;
		OffchainController = offchainController;
		IndexBuilderService = indexBuilderService;
		Config = config;
	}

	public BlockchainController BlockchainController { get; }
	public OffchainController OffchainController { get; }
	public IndexBuilderService IndexBuilderService { get; }
	public Config Config { get; }

	[HttpGet("synchronize")]
	[ResponseCache(Duration = 60)]
	public async Task<IActionResult> GetSynchronizeAsync(
		[FromQuery, Required] string bestKnownBlockHash,
		CancellationToken cancellationToken = default)
	{
		if (!uint256.TryParse(bestKnownBlockHash, out var knownHash))
		{
			return BadRequest($"Invalid {nameof(bestKnownBlockHash)}.");
		}

		var numberOfFilters = Config.Network == Network.Main ? 1000 : 10000;
		(Height bestHeight, IEnumerable<FilterModel> filters) = IndexBuilderService.GetFilterLinesExcluding(knownHash, numberOfFilters, out bool found);

		var response = new SynchronizeResponse { Filters = [], BestHeight = bestHeight };

		if (!found)
		{
			response.FiltersResponseState = FiltersResponseState.BestKnownHashNotFound;
		}
		else if (!filters.Any())
		{
			response.FiltersResponseState = FiltersResponseState.NoNewFilter;
		}
		else
		{
			response.FiltersResponseState = FiltersResponseState.NewFilters;
			response.Filters = filters;
		}

		try
		{
			response.AllFeeEstimate = await BlockchainController.GetAllFeeEstimateAsync(EstimateSmartFeeMode.Conservative, cancellationToken);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex);
		}

		response.ExchangeRates = await OffchainController.GetExchangeRatesCollectionAsync(cancellationToken);

		return Ok(response);
	}
}
