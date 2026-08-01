using Lyra.Commerce.Models;
using Lyra.Commerce.Services;
using Lyra.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Views;
using YesSql;

namespace Lyra.Commerce.Drivers;

/// <summary>
/// Renders the ProductGridWidget by querying the live catalog rather than storing product
/// references on the widget — dropping the widget onto a page (via a Layer, or as a FlowPart
/// block, including one placed by the AI Page Builder) always shows the current top N published
/// products, no re-editing needed as the catalog changes.
/// </summary>
public sealed class ProductGridWidgetPartDisplayDriver(ISession session) : ContentPartDisplayDriver<ProductGridWidgetPart>
{
    public override async Task<IDisplayResult> DisplayAsync(ProductGridWidgetPart part, BuildPartDisplayContext context)
    {
        var products = await session.Query<ContentItem, ContentItemIndex>(x =>
                x.ContentType == "Product" && x.Published)
            .OrderByDescending(x => x.CreatedUtc)
            .Take(part.MaxItems)
            .ListAsync();

        var summaries = products.Select(ProductProjection.ToSummary).ToList();

        return Initialize<ProductGridWidgetViewModel>("ProductGridWidgetPart", model =>
            model.Products = summaries).Location("Detail", "Content");
    }

    public override IDisplayResult Edit(ProductGridWidgetPart part, BuildPartEditorContext context) =>
        Initialize<ProductGridWidgetEditViewModel>("ProductGridWidgetPart_Edit", model =>
            model.MaxItems = part.MaxItems);

    public override async Task<IDisplayResult> UpdateAsync(ProductGridWidgetPart part, UpdatePartEditorContext context)
    {
        var model = new ProductGridWidgetEditViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.MaxItems = model.MaxItems is > 0 and <= 12 ? model.MaxItems : 3;

        return Edit(part, context);
    }
}
