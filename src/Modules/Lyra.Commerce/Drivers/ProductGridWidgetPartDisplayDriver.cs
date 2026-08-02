using Lyra.Commerce.Models;
using Lyra.Commerce.Services;
using Lyra.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media;
using YesSql;

namespace Lyra.Commerce.Drivers;

/// <summary>
/// Renders the ProductGridWidget by querying the live catalog rather than storing product
/// references on the widget — dropping the widget onto a page (via a Layer, or as a FlowPart
/// block, including one placed by the AI Page Builder) always shows the current top N published
/// products, no re-editing needed as the catalog changes.
/// </summary>
public sealed class ProductGridWidgetPartDisplayDriver(ISession session, IMediaFileStore mediaFileStore)
    : ContentPartDisplayDriver<ProductGridWidgetPart>
{
    public override async Task<IDisplayResult> DisplayAsync(ProductGridWidgetPart part, BuildPartDisplayContext context)
    {
        // The Category filter isn't backed by a SQL index (that would need a dedicated
        // TextFieldIndexProvider registration this project doesn't ship), so filtering happens
        // in-memory over the published catalog rather than in the query itself — fine at the
        // scale a demo storefront runs at, called out here so it isn't mistaken for an oversight.
        var candidates = await session.Query<ContentItem, ContentItemIndex>(x =>
                x.ContentType == "Product" && x.Published)
            .OrderByDescending(x => x.CreatedUtc)
            .ListAsync();

        var filtered = string.IsNullOrWhiteSpace(part.CategoryFilter)
            ? candidates
            : candidates.Where(p => MatchesCategory(p, part.CategoryFilter));

        var summaries = filtered
            .Take(part.MaxItems)
            .Select(p => ProductProjection.ToSummary(p, mediaFileStore))
            .ToList();

        return Initialize<ProductGridWidgetViewModel>("ProductGridWidgetPart", model =>
            model.Products = summaries).Location("Detail", "Content");
    }

    private static bool MatchesCategory(ContentItem product, string category)
    {
        dynamic part = product.Content.Product;
        var productCategory = (string?)part.Category?.Text;
        return string.Equals(productCategory, category, StringComparison.OrdinalIgnoreCase);
    }

    public override IDisplayResult Edit(ProductGridWidgetPart part, BuildPartEditorContext context) =>
        Initialize<ProductGridWidgetEditViewModel>("ProductGridWidgetPart_Edit", model =>
        {
            model.MaxItems = part.MaxItems;
            model.CategoryFilter = part.CategoryFilter;
        });

    public override async Task<IDisplayResult> UpdateAsync(ProductGridWidgetPart part, UpdatePartEditorContext context)
    {
        var model = new ProductGridWidgetEditViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.MaxItems = model.MaxItems is > 0 and <= 12 ? model.MaxItems : 3;
        part.CategoryFilter = string.IsNullOrWhiteSpace(model.CategoryFilter) ? null : model.CategoryFilter.Trim();

        return Edit(part, context);
    }
}
