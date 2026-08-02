using OrchardCore.Autoroute.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Settings;

namespace Lyra.Commerce;

public sealed class Migrations(IContentDefinitionManager contentDefinitionManager) : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await contentDefinitionManager.AlterPartDefinitionAsync("Product", part => part
            .WithField("Sku", f => f
                .OfType("TextField")
                .WithDisplayName("SKU"))
            .WithField("Description", f => f
                .OfType("HtmlField")
                .WithDisplayName("Description"))
            .WithField("Price", f => f
                .OfType("NumericField")
                .WithDisplayName("Price")
                .WithSettings(new NumericFieldSettings { Scale = 2, Minimum = 0 }))
            .WithField("StockQuantity", f => f
                .OfType("NumericField")
                .WithDisplayName("Stock quantity")
                .WithSettings(new NumericFieldSettings { Scale = 0, Minimum = 0, DefaultValue = "0" }))
            .WithField("TrackInventory", f => f
                .OfType("BooleanField")
                .WithDisplayName("Track inventory")));

        await contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
            .WithPart("TitlePart")
            .WithPart("AutoroutePart", p => p.WithSettings(new AutoroutePartSettings
            {
                Pattern = "products/{{ ContentItem.DisplayText | slugify }}",
                AllowCustomPath = true,
            }))
            .WithPart("Product")
            .Creatable()
            .Listable()
            .Draftable()
            .Versionable()
            .WithDisplayName("Product")
            .WithDescription("A product sold through the storefront — name, SKU, price and stock."));

        await contentDefinitionManager.AlterPartDefinitionAsync("ProductGridWidgetPart", part => part
            .Attachable()
            .WithDisplayName("Product grid"));

        await contentDefinitionManager.AlterTypeDefinitionAsync("ProductGridWidget", type => type
            .WithPart("ProductGridWidgetPart")
            .Stereotype("Widget")
            .Creatable()
            .WithDisplayName("Product grid")
            .WithDescription("Shows the most recently added published products."));

        return 1;
    }

    /// <summary>
    /// Adds a single product photo (MediaField, Multiple=false — one image is enough for the
    /// storefront card and admin thumbnail this module renders), a Currency field (a predefined
    /// list rather than free text, so the storefront can't end up with a typo'd currency code),
    /// and a Category field the storefront widget can filter by.
    /// </summary>
    public async Task<int> UpdateFrom1Async()
    {
        await contentDefinitionManager.AlterPartDefinitionAsync("Product", part => part
            .WithField("Image", f => f
                .OfType("MediaField")
                .WithDisplayName("Photo")
                .WithSettings(new MediaFieldSettings { Multiple = false, AllowMediaText = false }))
            .WithField("Currency", f => f
                .OfType("TextField")
                .WithDisplayName("Currency")
                .WithSettings(new TextFieldPredefinedListEditorSettings
                {
                    Options =
                    [
                        new ListValueOption { Value = "USD", Name = "USD" },
                        new ListValueOption { Value = "EUR", Name = "EUR" },
                        new ListValueOption { Value = "GBP", Name = "GBP" },
                        new ListValueOption { Value = "KES", Name = "KES" },
                    ],
                    DefaultValue = "USD",
                    Editor = EditorOption.Dropdown,
                }))
            .WithField("Category", f => f
                .OfType("TextField")
                .WithDisplayName("Category")));

        return 2;
    }

    /// <summary>
    /// WithEditor("PredefinedList") is what actually switches the Currency field's admin form
    /// input to the dropdown — TextFieldPredefinedListEditorSettings (added in UpdateFrom1Async)
    /// alone only supplies the options list; TextFieldPredefinedListEditorSettingsDriver checks
    /// the separate ContentPartFieldSettings.Editor string to decide whether to honor them at
    /// all. Missing this left the field rendering as a plain text box on already-provisioned
    /// tenants despite the options being saved correctly — caught by re-checking the live admin
    /// form's rendered HTML, not by re-reading the settings JSON, which looked fine either way.
    /// </summary>
    public async Task<int> UpdateFrom2Async()
    {
        await contentDefinitionManager.AlterPartDefinitionAsync("Product", part => part
            .WithField("Currency", f => f.WithEditor("PredefinedList")));

        return 3;
    }
}
