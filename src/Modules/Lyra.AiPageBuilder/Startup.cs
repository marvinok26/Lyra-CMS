using Lyra.AiPageBuilder.Abstractions;
using Lyra.AiPageBuilder.Options;
using Lyra.AiPageBuilder.Providers;
using Lyra.AiPageBuilder.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace Lyra.AiPageBuilder;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<AiPageBuilderOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                configuration.GetSection(AiPageBuilderOptions.SectionName).Bind(options));

        // "Mock" first and always registered, so generation works with zero configuration —
        // real providers are additive, never a requirement.
        services.AddScoped<IAiProvider, MockAiProvider>();
        services.AddHttpClient<IAiProvider, OpenAiProvider>();
        services.AddHttpClient<IAiProvider, AnthropicProvider>();

        services.AddScoped<PageGenerationOrchestrator>();
        services.AddScoped<ContentPlanApplier>();

        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
    }
}
