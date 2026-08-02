namespace Lyra.Commerce.ViewModels;

public class ProductGridWidgetViewModel
{
    public IReadOnlyList<ProductSummaryViewModel> Products { get; set; } = [];
}

public class ProductGridWidgetEditViewModel
{
    public int MaxItems { get; set; } = 3;
    public string? CategoryFilter { get; set; }
}
