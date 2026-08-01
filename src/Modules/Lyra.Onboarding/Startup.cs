using Lyra.Onboarding.Options;
using Lyra.Onboarding.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Lyra.Onboarding;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<OnboardingOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                configuration.GetSection(OnboardingOptions.SectionName).Bind(options));

        services.AddScoped<TenantDatabaseInitializer>();
        services.AddScoped<StoreProvisioner>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
    }
}
