using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin.RPC;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc.Formatters;
using UnchainexWallet.Backend.Middlewares;
using UnchainexWallet.BitcoinRpc;
using UnchainexWallet.Blockchain.BlockFilters;
using UnchainexWallet.Blockchain.Blocks;
using UnchainexWallet.Blockchain.Mempool;
using UnchainexWallet.Cache;
using UnchainexWallet.Extensions;
using UnchainexWallet.Helpers;
using UnchainexWallet.Interfaces;
using UnchainexWallet.Logging;
using UnchainexWallet.Serialization;
using UnchainexWallet.Userfacing;
using UnchainexWallet.WebClients;

[assembly: ApiController]

namespace UnchainexWallet.Backend;

public class Startup
{
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		string dataDir = Configuration["datadir"] ?? EnvironmentHelpers.GetDataDir(Path.Combine("UnchainexWallet", "Backend"));
		Logger.InitializeDefaults(Path.Combine(dataDir, "Logs.txt"));

		services.AddMemoryCache();
		services.AddMvc(options =>
			{
				options.OutputFormatters.Insert(0, new UnchainexJsonOutputFormatter(Encode.BackendMessage));
				options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
				options.InputFormatters.RemoveType<NewtonsoftJsonInputFormatter>();
				options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
				options.OutputFormatters.RemoveType<NewtonsoftJsonOutputFormatter>();
			})
			.AddControllersAsServices();

		services.AddControllers();

		services.AddSingleton<IExchangeRateProvider>(new ExchangeRateProvider());
		string configFilePath = Path.Combine(dataDir, "Config.json");
		Config config = Config.LoadFile(configFilePath);
		services.AddSingleton(serviceProvider => config );

		services.AddSingleton<IdempotencyRequestCache>();
		services.AddHttpClient("no-name").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
		{
			// See https://github.com/dotnet/runtime/issues/18348#issuecomment-415845645
			PooledConnectionLifetime = TimeSpan.FromMinutes(5)
		});
		services.AddSingleton<IRPCClient>(provider =>
		{
			string host = config.GetBitcoinCoreRpcEndPoint().ToString(config.Network.RPCPort);
			RPCClient rpcClient = new(
					authenticationString: config.BitcoinRpcConnectionString,
					hostOrUri: host,
					network: config.Network);

			IMemoryCache memoryCache = provider.GetRequiredService<IMemoryCache>();
			CachedRpcClient cachedRpc = new(rpcClient, memoryCache);
			return cachedRpc;
		});

		var network = config.Network;

		services.AddSingleton(_ => network);
		services.AddBackgroundService<BlockNotifier>();
		services.AddSingleton<MempoolService>();
		services.AddSingleton<IdempotencyRequestCache>();
		services.AddSingleton<IndexBuilderService>(s =>
			new IndexBuilderService(
				s.GetRequiredService<IRPCClient>(),
				s.GetRequiredService<BlockNotifier>(),
				Path.Combine(dataDir, "IndexBuilderService", $"Index{network}.sqlite")
				));
		services.AddStartupTask<StartupTask>();
		services.AddResponseCompression();
		services.AddRequestTimeouts(options =>
			options.DefaultPolicy =
				new RequestTimeoutPolicy
				{
					Timeout = TimeSpan.FromSeconds(5)
				});
	}

	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This method gets called by the runtime. Use this method to configure the HTTP request pipeline")]
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		app.UseRouting();

		// So to correctly handle HEAD requests.
		// https://www.tpeczek.com/2017/10/exploring-head-method-behavior-in.html
		// https://github.com/tpeczek/Demo.AspNetCore.Mvc.CosmosDB/blob/master/Demo.AspNetCore.Mvc.CosmosDB/Middlewares/HeadMethodMiddleware.cs
		app.UseMiddleware<HeadMethodMiddleware>();

		app.UseResponseCompression();

		app.UseEndpoints(endpoints => endpoints.MapControllers());
		app.UseRequestTimeouts();
	}
}
