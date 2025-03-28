using System.Collections.Generic;
using UnchainexWallet.Blockchain.Analysis.FeesEstimation;

namespace UnchainexWallet.Backend.Models.Responses;

public class SynchronizeResponse
{
	public FiltersResponseState FiltersResponseState { get; set; }

	public IEnumerable<FilterModel> Filters { get; set; } = new List<FilterModel>();

	public int BestHeight { get; set; }

	public AllFeeEstimate? AllFeeEstimate { get; set; }

	public IEnumerable<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
}
