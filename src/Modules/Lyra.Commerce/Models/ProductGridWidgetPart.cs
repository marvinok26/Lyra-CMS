using OrchardCore.ContentManagement;

namespace Lyra.Commerce.Models;

/// <summary>
/// A marker part for the ProductGridWidget content type — it carries no product data itself,
/// only the display settings for which of the live, published Products to pull in and render.
/// The actual products are queried live at display time (see ProductGridWidgetPartDisplayDriver),
/// not stored on the widget.
/// </summary>
public sealed class ProductGridWidgetPart : ContentPart
{
    public int MaxItems { get; set; } = 3;

    /// <summary>When set, only products whose Category field matches this value (case-insensitive)
    /// are shown — an empty value means "no filter", every published product is eligible.</summary>
    public string? CategoryFilter { get; set; }
}
