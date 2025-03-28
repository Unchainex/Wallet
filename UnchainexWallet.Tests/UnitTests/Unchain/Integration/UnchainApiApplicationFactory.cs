using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBitcoin;
using UnchainexWallet.BitcoinRpc;
using UnchainexWallet.Crypto.Randomness;
using UnchainexWallet.Tests.Helpers;
using UnchainexWallet.Unchain.Backend;
using UnchainexWallet.Unchain.Backend.DoSPrevention;
using UnchainexWallet.Unchain.Backend.Rounds;
using UnchainexWallet.Unchain.Backend.Statistics;
using UnchainexWallet.Unchain.Client;
using UnchainexWallet.Unchain.Models;
using UnchainexWallet.Unchain.Models.MultipartyTransaction;
using UnchainexWallet.Services;
using UnchainexWallet.Unchain.Client.CoinJoin.Client;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Integration;

public class UnchainApiApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
	// There is a deadlock in the current version of the asp.net testing framework
	// https://www.strathweb.com/2021/05/the-curious-case-of-asp-net-core-integration-test-deadlock/
	protected override IHost CreateHost(IHostBuilder builder)
	{
		var host = builder.Build();
		Task.Run(() => host.StartAsync()).GetAwaiter().GetResult();
		return host;
	}

	protected override void ConfigureClient(HttpClient client)
	{
		client.Timeout = TimeSpan.FromMinutes(10);
	}

	protected override IHostBuilder CreateHostBuilder()
	{
		var builder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(x => x.UseStartup<TStartup>().UseTestServer());
		return builder;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		// will be called after the `ConfigureServices` from the Startup
		builder.ConfigureTestServices(services =>
		{
			services.AddHostedService<BackgroundServiceStarter<Arena>>();
			services.AddSingleton<Arena>();
			services.AddSingleton(_ => Network.RegTest);
			services.AddSingleton<IRPCClient>(_ => BitcoinFactory.GetMockMinimalRpc());
			services.AddSingleton<Prison>(_ => UnchainFactory.CreatePrison());
			services.AddSingleton<UnchainConfig>();
			services.AddSingleton<RoundParameterFactory>();
			services.AddSingleton(typeof(TimeSpan), _ => TimeSpan.FromSeconds(2));
			services.AddSingleton(s => new CoinJoinScriptStore());
			services.AddSingleton<CoinJoinFeeRateStatStore>(s =>
				CoinJoinFeeRateStatStore.LoadFromFile(
					"./CoinJoinFeeRateStatStore.txt",
					s.GetRequiredService<UnchainConfig>(),
					s.GetRequiredService<IRPCClient>()
					));
			services.AddHttpClient();
		});
		builder.ConfigureLogging(o => o.SetMinimumLevel(LogLevel.Warning));
	}

	public Task<ArenaClient> CreateArenaClientAsync(HttpClient httpClient) =>
		CreateArenaClientAsync(CreateUnchainHttpApiClient(httpClient));

	public async Task<ArenaClient> CreateArenaClientAsync(UnchainHttpApiClient unChainHttpApiClient)
	{
		var rounds = (await unChainHttpApiClient.GetStatusAsync(RoundStateRequest.Empty, CancellationToken.None)).RoundStates;
		var round = rounds.First(x => x.CoinjoinState is ConstructionState);
		var arenaClient = new ArenaClient(
			round.CreateAmountCredentialClient(InsecureRandom.Instance),
			round.CreateVsizeCredentialClient(InsecureRandom.Instance),
			round.CoinjoinState.Parameters.CoordinationIdentifier,
			unChainHttpApiClient);
		return arenaClient;
	}

	public UnchainHttpApiClient CreateUnchainHttpApiClient(HttpClient httpClient) =>
		new("identity", new MockHttpClientFactory { OnCreateClient = _ => httpClient});
}
