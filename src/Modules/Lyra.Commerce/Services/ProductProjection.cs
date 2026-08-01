using Lyra.Commerce.ViewModels;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace Lyra.Commerce.Services;

/// <summary>
/// Projects a "Product" content item's stock-field data (Product.Sku.Text, Product.Price.Value,
/// etc. — confirmed against the actual field-storage shape, the same care taken for the AI page
/// builder's widgets in Lyra.AiPageBuilder) into a plain view model, shared by the admin product
/// list and the storefront ProductGridWidget so both read the fields the same way.
/// </summary>
public static class ProductProjection
{
    public static ProductSummaryViewModel ToSummary(ContentItem product)
    {
        dynamic part = product.Content.Product;

        return new ProductSummaryViewModel
        {
            ContentItemId = product.ContentItemId,
            Name = product.As<TitlePart>()?.Title ?? product.DisplayText,
            Sku = (string?)part.Sku?.Text,
            Price = (decimal?)part.Price?.Value,
            StockQuantity = (decimal?)part.StockQuantity?.Value,
            TrackInventory = (bool?)part.TrackInventory?.Value ?? false,
            Path = product.As<AutoroutePart>()?.Path,
        };
    }
}
