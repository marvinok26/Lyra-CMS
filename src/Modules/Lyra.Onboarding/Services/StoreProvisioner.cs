using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using OrchardCore.Themes.Services;

namespace Lyra.Onboarding.Services;

/// <summary>
/// Everything a tenant needs beyond the bare "SaaS" recipe (proven in Phase 3 to enable nothing
/// content-related — no Contents, ContentFields, Autoroute, Title, Widgets, or Layers, and no
/// theme) so a self-service signup lands on a tenant that can actually be used immediately: the
/// content type vocabulary the AI Page Builder and Commerce module already target, a working
/// "Always" layer (an empty condition list matches nothing — also learned the hard way in Phase 3),
/// and the public theme applied.
/// </summary>
public sealed class StoreProvisioner
{
    private static readonly string[] FeaturesToEnable =
    [
        "OrchardCore.Contents",
        "OrchardCore.ContentTypes",
        "OrchardCore.ContentFields",
        "OrchardCore.Title",
        "OrchardCore.Autoroute",
        "OrchardCore.Flows",
        "OrchardCore.Widgets",
        "OrchardCore.Layers",
        "Lyra.PublicTheme",
        "Lyra.Commerce",
        "Lyra.AiPageBuilder",
    ];

    public async Task ProvisionAsync(string tenantName)
    {
        // Enabling features rebuilds the tenant's DI container (new services become available,
        // e.g. IContentDefinitionManager once OrchardCore.ContentTypes is on) — but only for a
        // *new* scope. Doing the rest of the provisioning in the same scope that issued the
        // enable throws "No service for type ... has been registered", since that scope's
        // container was already built before the enable took effect. A fresh child scope forces
        // Orchard to notice the shell descriptor changed and rebuild.
        await ShellScope.UsingChildScopeAsync(tenantName, async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await featuresManager.GetAvailableFeaturesAsync();
            var toEnable = available.Where(f => FeaturesToEnable.Contains(f.Id)).ToArray();
            await featuresManager.UpdateFeaturesAsync([], toEnable, force: true);
        });

        await ShellScope.UsingChildScopeAsync(tenantName, async scope =>
        {
            var themeService = scope.ServiceProvider.GetRequiredService<ISiteThemeService>();
            await themeService.SetSiteThemeAsync("Lyra.PublicTheme");

            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            await DefineWidgetTypesAsync(contentDefinitionManager);
            await DefinePageTypeAsync(contentDefinitionManager);

            await CreateAlwaysLayerAsync(scope);
        });
    }

    /// <summary>
    /// Paragraph and RawHtml: a part named after the type itself, holding one HtmlField named
    /// "Content" — the exact shape Lyra.AiPageBuilder's ContentPlanApplier already assumes
    /// (confirmed against the running app in Phase 2, not re-derived here).
    /// </summary>
    private static async Task DefineWidgetTypesAsync(IContentDefinitionManager contentDefinitionManager)
    {
        foreach (var typeName in new[] { "Paragraph", "RawHtml" })
        {
            await contentDefinitionManager.AlterPartDefinitionAsync(typeName, part => part
                .Attachable()
                .WithField("Content", f => f.OfType("HtmlField").WithDisplayName("Content")));

            await contentDefinitionManager.AlterTypeDefinitionAsync(typeName, type => type
                .WithPart(typeName)
                .Stereotype("Widget")
                .Creatable()
                .WithDisplayName(typeName));
        }
    }

    private static async Task DefinePageTypeAsync(IContentDefinitionManager contentDefinitionManager)
    {
        await contentDefinitionManager.AlterTypeDefinitionAsync("Page", type => type
            .WithPart("TitlePart")
            .WithPart("AutoroutePart", p => p.WithSettings(new AutoroutePartSettings
            {
                Pattern = "{{ ContentItem.DisplayText | slugify }}",
                ShowHomepageOption = true,
                AllowCustomPath = true,
            }))
            .WithPart("FlowPart")
            .Creatable()
            .Listable()
            .Draftable()
            .Versionable()
            .WithDisplayName("Page"));
    }

    /// <summary>
    /// A hand-created layer's rule defaults to an empty condition list, which matches nothing —
    /// the built-in "Always" layer that ships with content-bearing recipes carries an explicit
    /// BooleanCondition set to true, so this replicates that instead of leaving widgets invisible.
    /// </summary>
    private static async Task CreateAlwaysLayerAsync(ShellScope scope)
    {
        var layerService = scope.ServiceProvider.GetRequiredService<ILayerService>();
        var conditionIdGenerator = scope.ServiceProvider.GetRequiredService<IConditionIdGenerator>();

        var layers = await layerService.LoadLayersAsync();
        if (layers.Layers.Any(l => l.Name == "Always")) return;

        var alwaysTrue = new BooleanCondition { Name = "BooleanCondition", Value = true };
        conditionIdGenerator.GenerateUniqueId(alwaysTrue);

        var rule = new Rule();
        conditionIdGenerator.GenerateUniqueId(rule);
        rule.Conditions.Add(alwaysTrue);

        layers.Layers.Add(new Layer
        {
            Name = "Always",
            Description = "The widgets in this layer are displayed on any page of this site.",
            LayerRule = rule,
        });

        await layerService.UpdateAsync(layers);
    }
}
