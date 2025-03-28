using System.Threading;
using System.Threading.Tasks;
using Xunit;
using UnchainexWallet.FeeRateEstimation;
using UnchainexWallet.WebClients.Unchainex;
using UnchainexWallet.Wallets.Exchange;

namespace UnchainexWallet.Tests.IntegrationTests;

public class ExternalApiTests
{
	[Fact]
	public async Task MempoolSpaceExchangeRateProviderTestsAsync() =>
		await AssertProviderAsync("MempoolSpace");

	[Fact]
	public async Task BlockchainInfoExchangeRateProviderTestsAsync() =>
		await AssertProviderAsync("BlockchainInfo");

	[Fact]
	public async Task CoinGeckoExchangeRateProviderTestsAsync() =>
		await AssertProviderAsync("CoinGecko");

	[Fact]
	public async Task GeminiExchangeRateProviderTestsAsync() =>
		await AssertProviderAsync("Gemini");

	private async Task AssertProviderAsync(string providerName)
	{
		using CancellationTokenSource timeoutCts = new(TimeSpan.FromMinutes(3));
		var provider = new ExchangeRateProvider(new HttpClientFactory());
		var userAgent = WebClients.UserAgent.GetNew(Random.Shared.Next());
		var rate = await provider.GetExchangeRateAsync(providerName, userAgent, timeoutCts.Token).ConfigureAwait(false);
		Assert.NotEqual(0m, rate.Rate);
	}

	[Fact]
	public async Task BlockstreamFeeRateProviderTestsAsync() =>
		await AssertFeeProviderAsync("BlockstreamInfo");

	[Fact]
	public async Task MempoolSpaceRateProviderTestsAsync() =>
		await AssertFeeProviderAsync("MempoolSpace");

	private async Task AssertFeeProviderAsync(string providerName)
	{
		using CancellationTokenSource timeoutCts = new(TimeSpan.FromMinutes(3));
		var provider = new FeeRateProvider(new HttpClientFactory());
		var userAgent = WebClients.UserAgent.GetNew(Random.Shared.Next());
		userAgent = WebClients.UserAgent.TrimUserAgent(userAgent);
		var estimations = await provider.GetFeeRateEstimationsAsync(providerName, userAgent, timeoutCts.Token).ConfigureAwait(false);
		Assert.NotEmpty(estimations.Estimations);
	}
}
