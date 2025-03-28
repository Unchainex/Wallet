using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using UnchainexWallet.Cache;
using UnchainexWallet.Coordinator;
using UnchainexWallet.Coordinator.Controllers;
using UnchainexWallet.Serialization;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Integration;

public class Startup
{
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		app.UseRouting();
		app.UseEndpoints(endpoints => endpoints.MapControllers());
	}

	public void ConfigureServices(IServiceCollection services)
	{
		var backendAssembly = typeof(UnchainController).Assembly;
		services.AddSingleton<IdempotencyRequestCache>();
		services
			.AddMvc(options =>
			{
				options.InputFormatters.Insert(0, new UnchainexJsonInputFormatter(Decode.CoordinatorMessageFromStreamAsync));
				options.OutputFormatters.Insert(0, new UnchainexJsonOutputFormatter(Encode.CoordinatorMessage));
				options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Script)));
			})
			.ConfigureApplicationPartManager(manager =>
			{
				manager.FeatureProviders.Add(new ControllerProvider(Configuration));
			})
			.AddApplicationPart(backendAssembly);
	}
}
public class ControllerProvider : ControllerFeatureProvider
{
    public readonly IConfiguration _configuration;

    public ControllerProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override bool IsController(TypeInfo typeInfo)
    {
	    return typeInfo.Name.Contains(nameof(UnchainController));
    }
}
