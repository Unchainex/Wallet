using NBitcoin;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnchainexWallet.Services;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Tests.XunitConfiguration;
using UnchainexWallet.Tor;
using UnchainexWallet.WebClients.Unchainex;
using Xunit;

namespace UnchainexWallet.Tests.IntegrationTests;

[Collection("LiveServerTests collection")]
public class LiveServerTests : IAsyncLifetime
{
	public LiveServerTests(LiveServerTestsFixture liveServerTestsFixture)
	{
		LiveServerTestsFixture = liveServerTestsFixture;
		HttpClientFactory = new OnionHttpClientFactory(new Uri($"socks5://{Common.TorSocks5Endpoint}"));
		TorProcessManager = new(Common.TorSettings, new EventBus());
	}

	private TorProcessManager TorProcessManager { get; }
	private IHttpClientFactory HttpClientFactory { get; }
	private LiveServerTestsFixture LiveServerTestsFixture { get; }

	public async Task InitializeAsync()
	{
		using CancellationTokenSource startTimeoutCts = new(TimeSpan.FromMinutes(2));

		await TorProcessManager.StartAsync(startTimeoutCts.Token);
	}

	public async Task DisposeAsync()
	{
		await TorProcessManager.DisposeAsync();
	}

	[Theory]
	[MemberData(nameof(GetNetworks))]
	public async Task GetBackendVersionTestsAsync(Network network)
	{
		using CancellationTokenSource ctsTimeout = new(TimeSpan.FromMinutes(2));

		UnchainexClient client = MakeUnchainexClient(network);
		var backendMajorVersion = await client.GetBackendMajorVersionAsync(ctsTimeout.Token);
		Assert.Equal(4, backendMajorVersion);
	}

	private UnchainexClient MakeUnchainexClient(Network network)
	{
		HttpClient httpClient = HttpClientFactory.CreateClient();
		httpClient.BaseAddress =LiveServerTestsFixture.UriMappings[network];
		return new UnchainexClient(httpClient);
	}

	public static IEnumerable<object[]> GetNetworks()
	{
		yield return new object[] { Network.Main };
		yield return new object[] { Network.TestNet };
	}
}
