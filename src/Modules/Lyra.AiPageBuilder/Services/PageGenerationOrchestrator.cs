using Lyra.AiPageBuilder.Abstractions;
using Lyra.AiPageBuilder.Options;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace Lyra.AiPageBuilder.Services;

/// <summary>
/// The one entry point the admin controller calls. Resolves the active IAiProvider, queries this
/// tenant's actual widget catalog (so generation is always scoped to what's really installed, not
/// a fixed list baked into the module), and drops any widget the provider returns that isn't in
/// that catalog rather than failing the whole generation — a bad single block shouldn't sink an
/// otherwise-good page.
/// </summary>
public sealed class PageGenerationOrchestrator(
    IContentDefinitionManager contentDefinitionManager,
    IEnumerable<IAiProvider> providers,
    IOptions<AiPageBuilderOptions> options)
{
    public async Task<PageGenerationPlan> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        var widgetTypes = (await contentDefinitionManager.ListWidgetTypeDefinitionsAsync())
            .Select(t => t.Name)
            .ToList();

        var activeProviderName = options.Value.ActiveProvider;
        var provider = providers.FirstOrDefault(p => p.Name == activeProviderName)
            ?? providers.First(p => p.Name == "Mock");

        var plan = await provider.GeneratePageAsync(new PageGenerationRequest
        {
            Prompt = prompt,
            AvailableWidgetTypes = widgetTypes,
        }, ct);

        // Defensive even against providers using structured-output modes: a plan built against a
        // stale catalog snapshot, or a provider not enforcing its schema, must degrade gracefully.
        plan.Widgets = plan.Widgets.Where(w => widgetTypes.Contains(w.ContentType)).ToList();

        return plan;
    }
}
