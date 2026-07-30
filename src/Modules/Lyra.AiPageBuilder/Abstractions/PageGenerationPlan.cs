namespace Lyra.AiPageBuilder.Abstractions;

/// <summary>
/// The generated blueprint for one page: a title plus an ordered list of content blocks, each
/// targeting one of the tenant's actual widget content types. Deliberately simpler than a full
/// zone/layout schema for v1 — Paragraph and RawHtml both share the same single-HTML-field shape
/// (a part named after the type, holding Content.Html), so one generic WidgetBlock covers a real
/// hero + features + pull-quote page without needing per-type field mapping yet. Not every stock
/// widget fits this: Blockquote's part holds a plain TextField instead, so it's deliberately not
/// targeted (see MockAiProvider's comment). A future widget with a richer shape (e.g.
/// Lyra.Commerce's product grid) is a natural place to extend this, not a redesign of it.
/// </summary>
public sealed class PageGenerationPlan
{
    public string PageTitle { get; set; } = string.Empty;
    public List<WidgetBlock> Widgets { get; set; } = [];
}

public sealed class WidgetBlock
{
    /// <summary>Must be one of the content types passed in PageGenerationRequest.AvailableWidgetTypes.</summary>
    public string ContentType { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
}
