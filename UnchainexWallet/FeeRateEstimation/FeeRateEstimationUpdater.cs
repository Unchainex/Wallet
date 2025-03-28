using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Bases;
using UnchainexWallet.Logging;
using UnchainexWallet.Services;
using UnchainexWallet.WebClients;

namespace UnchainexWallet.FeeRateEstimation;

public class FeeRateEstimationUpdater :  PeriodicRunner
{
	private readonly Func<string> _feeRateProviderGetter;
	private readonly EventBus _eventBus;
	private readonly FeeRateProvider _provider;
	private readonly UserAgentPicker _userAgentPicker;

	public FeeRateEstimations? FeeEstimates { get; private set; }

	public FeeRateEstimationUpdater(TimeSpan period, Func<string> feeRateProviderGetter, IHttpClientFactory httpClientFactory, EventBus eventBus)
		: base(period)
	{
		_provider = new FeeRateProvider(httpClientFactory);
		_feeRateProviderGetter = feeRateProviderGetter;
		_userAgentPicker = UserAgent.GenerateUserAgentPicker(false);
		_eventBus = eventBus;
	}

	protected override async Task ActionAsync(CancellationToken cancellationToken)
	{
		var newFeeRateEstimations = await _provider.GetFeeRateEstimationsAsync(_feeRateProviderGetter(), _userAgentPicker(), cancellationToken).ConfigureAwait(false);
		if (newFeeRateEstimations != FeeEstimates)
		{
			Logger.LogInfo($"Fetched fee rate estimations from {_feeRateProviderGetter()}.");

			FeeEstimates = newFeeRateEstimations;
			_eventBus.Publish(new MiningFeeRatesChanged(newFeeRateEstimations));
		}

		await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(120)), cancellationToken).ConfigureAwait(false);
	}
}
