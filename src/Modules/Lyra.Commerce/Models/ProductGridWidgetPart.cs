using OrchardCore.ContentManagement;

namespace Lyra.Commerce.Models;

/// <summary>
/// A marker part for the ProductGridWidget content type — it carries no product data itself,
/// only the display setting for how many of the most recently created published Products to
/// pull in and render. The actual products are queried live at display time (see
/// ProductGridWidgetPartDisplayDriver), not stored on the widget.
/// </summary>
public sealed class ProductGridWidgetPart : ContentPart
{
    public int MaxItems { get; set; } = 3;
}
