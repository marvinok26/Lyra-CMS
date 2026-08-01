namespace Lyra.Commerce.ViewModels;

public sealed class ProductSummaryViewModel
{
    public required string ContentItemId { get; init; }
    public required string Name { get; init; }
    public string? Sku { get; init; }
    public decimal? Price { get; init; }
    public decimal? StockQuantity { get; init; }
    public bool TrackInventory { get; init; }
    public string? Path { get; init; }
}
