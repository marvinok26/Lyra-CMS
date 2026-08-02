using Lyra.Commerce.ViewModels;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Media;
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
    public static ProductSummaryViewModel ToSummary(ContentItem product, IMediaFileStore mediaFileStore)
    {
        dynamic part = product.Content.Product;

        return new ProductSummaryViewModel
        {
            ContentItemId = product.ContentItemId,
            Name = product.As<TitlePart>()?.Title ?? product.DisplayText,
            Sku = (string?)part.Sku?.Text,
            Price = (decimal?)part.Price?.Value,
            Currency = (string?)part.Currency?.Text,
            Category = (string?)part.Category?.Text,
            StockQuantity = (decimal?)part.StockQuantity?.Value,
            TrackInventory = (bool?)part.TrackInventory?.Value ?? false,
            Path = product.As<AutoroutePart>()?.Path,
            ImageUrl = ResolveImageUrl(part, mediaFileStore),
        };
    }

    /// <summary>
    /// MediaField.Paths is a string[] relative to the media store — resolving it to a public URL
    /// needs IMediaFileStore.MapPathToPublicUrl, the same abstraction Orchard's own media pickers
    /// use, so this works the same whether media is on local disk, Azure Blob, or S3.
    /// </summary>
    private static string? ResolveImageUrl(dynamic productPart, IMediaFileStore mediaFileStore)
    {
        dynamic? image = productPart.Image;
        if (image is null) return null;

        dynamic? paths = image.Paths;
        if (paths is null) return null;

        // Each element comes back as a dynamic wrapper (JsonDynamicValue), not a plain string, so
        // `path is string` pattern-matches false even for a real path — an explicit cast is
        // required, the same dynamic-JSON gotcha documented elsewhere in this project (e.g.
        // Lyra.AiPageBuilder's ExtractHtml). Caught by checking the actual rendered admin/storefront
        // HTML, not by re-reading the stored JSON, which looked correct either way.
        foreach (var pathValue in paths)
        {
            string? relativePath = (string?)pathValue;
            if (!string.IsNullOrEmpty(relativePath))
                return mediaFileStore.MapPathToPublicUrl(relativePath);
        }

        return null;
    }
}
