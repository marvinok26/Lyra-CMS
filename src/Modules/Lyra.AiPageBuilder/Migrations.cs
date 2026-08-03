using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace Lyra.AiPageBuilder;

/// <summary>
/// Paragraph and RawHtml aren't defined by this module — they're stock Orchard content types
/// (from OrchardCore.Widgets/Contents), except on self-service tenants where
/// Lyra.Onboarding.StoreProvisioner defines them itself because the bare "SaaS" recipe doesn't.
/// Either way, AlterPartDefinitionAsync merges into whatever already exists, so this applies
/// cleanly regardless of who defined the type first.
///
/// Default editor for an HtmlField is a plain CodeMirror textarea — fine for hand-authored HTML,
/// hostile for reviewing an AI-generated page. Switching to Trumbowyg (WYSIWYG) doesn't lose raw-
/// HTML access: Trumbowyg's default toolbar (bundled in OrchardCore.Resources) ships a "View HTML"
/// button as a core button, not an opt-in plugin — confirmed by inspecting the bundled trumbowyg.js
/// defaultOptions.btns array, not assumed. That matters here specifically because the AI Page
/// Builder writes structural markup (hero/feature-grid divs) that a naive rich-text editor would
/// otherwise strip on save.
/// </summary>
public sealed class Migrations(IContentDefinitionManager contentDefinitionManager) : DataMigration
{
    public async Task<int> CreateAsync()
    {
        foreach (var typeName in new[] { "Paragraph", "RawHtml" })
        {
            await contentDefinitionManager.AlterPartDefinitionAsync(typeName, part => part
                .WithField("Content", f => f.WithEditor("Trumbowyg")));
        }

        return 1;
    }
}
