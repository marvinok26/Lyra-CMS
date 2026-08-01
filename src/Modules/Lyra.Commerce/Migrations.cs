using OrchardCore.Autoroute.Models;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

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
}
