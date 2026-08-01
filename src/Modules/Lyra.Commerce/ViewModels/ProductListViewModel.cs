namespace Lyra.Commerce.ViewModels;

public sealed class ProductListViewModel
{
    public IReadOnlyList<ProductSummaryViewModel> Products { get; init; } = [];
}
