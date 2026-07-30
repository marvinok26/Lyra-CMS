using System.Text;
using System.Text.Json.Nodes;
using Lyra.AiPageBuilder.Abstractions;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Flows.Models;
using OrchardCore.Title.Models;

namespace Lyra.AiPageBuilder.Services;

/// <summary>
/// Turns a validated PageGenerationPlan into real Orchard Core content: a Page whose FlowPart
/// embeds one widget content item per plan entry, in order. Created as Draft — never auto-published
/// — so a tenant admin reviews the generated page before it goes live; publishing it is a separate,
/// explicit action (see AiPageBuilderAdminController.Publish).
/// </summary>
public sealed class ContentPlanApplier(IContentManager contentManager)
{
    public async Task<ContentItem> ApplyAsync(PageGenerationPlan plan, CancellationToken ct = default)
    {
        var page = await contentManager.NewAsync("Page");

        page.Alter<TitlePart>(x => x.Title = plan.PageTitle);
        page.Alter<AutoroutePart>(x => x.Path = Slugify(plan.PageTitle));

        // Built up front (with each NewAsync properly awaited) rather than inside the Alter<T>
        // callback, which takes a synchronous Action<T> — an async lambda there would run as
        // fire-and-forget instead of completing before CreateAsync below.
        var widgetItems = new List<ContentItem>(plan.Widgets.Count);
        foreach (var widgetPlan in plan.Widgets)
        {
            var widget = await contentManager.NewAsync(widgetPlan.ContentType);
            SetHtmlBody(widget, widgetPlan.ContentType, widgetPlan.Html);
            widgetItems.Add(widget);
        }

        page.Alter<FlowPart>(flow =>
        {
            foreach (var widget in widgetItems) flow.Widgets.Add(widget);
        });

        await contentManager.CreateAsync(page, VersionOptions.Draft);
        return page;
    }

    /// <summary>
    /// The stock widget types this provider targets (Paragraph, RawHtml, Blockquote) each define a
    /// part named after the content type itself, holding a single HTML field named "Content" —
    /// confirmed by inspecting the actual admin form field names (e.g. "Paragraph.Content.Html")
    /// rather than assumed, since none of them have a dedicated strongly-typed part class to
    /// Alter&lt;T&gt; against.
    /// </summary>
    private static void SetHtmlBody(ContentItem widget, string contentType, string html)
    {
        widget.Content[contentType] = new JsonObject
        {
            ["Content"] = new JsonObject { ["Html"] = html },
        };
    }

    private static string Slugify(string title)
    {
        var builder = new StringBuilder();
        foreach (var c in title.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c)) builder.Append(c);
            else if (builder.Length > 0 && builder[^1] != '-') builder.Append('-');
        }
        var slug = builder.ToString().Trim('-');
        return slug.Length == 0 ? $"page-{Guid.NewGuid():N}"[..12] : slug;
    }
}
