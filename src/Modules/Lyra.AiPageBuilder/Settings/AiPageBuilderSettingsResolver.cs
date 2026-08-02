using Lyra.AiPageBuilder.Options;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace Lyra.AiPageBuilder.Settings;

public sealed record AiPageBuilderEffectiveSettings(string ActiveProvider, string? ApiKey, string? Model, string OllamaBaseUrl);

/// <summary>
/// Merges the per-tenant AiPageBuilderSettings (from ISite, set via Settings → AI Page Builder)
/// over the host-level AiPageBuilderOptions (from configuration/environment) — a tenant only
/// needs to set what it wants to override, everything else falls back to the host default.
/// </summary>
public sealed class AiPageBuilderSettingsResolver(ISiteService siteService, IOptions<AiPageBuilderOptions> hostOptions)
{
    public async Task<AiPageBuilderEffectiveSettings> GetEffectiveSettingsAsync()
    {
        var host = hostOptions.Value;
        var site = await siteService.GetSiteSettingsAsync();
        site.TryGet<AiPageBuilderSettings>(out var tenant);

        return new AiPageBuilderEffectiveSettings(
            ActiveProvider: FirstNonEmpty(tenant?.ActiveProvider, host.ActiveProvider) ?? "Mock",
            ApiKey: FirstNonEmpty(tenant?.ApiKey, host.ApiKey),
            Model: FirstNonEmpty(tenant?.Model, host.Model),
            OllamaBaseUrl: FirstNonEmpty(tenant?.OllamaBaseUrl, host.OllamaBaseUrl) ?? "http://localhost:11434");
    }

    private static string? FirstNonEmpty(string? first, string? second) =>
        string.IsNullOrWhiteSpace(first) ? (string.IsNullOrWhiteSpace(second) ? null : second) : first;
}
