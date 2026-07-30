using System.Text.RegularExpressions;
using Lyra.AiPageBuilder.Abstractions;

namespace Lyra.AiPageBuilder.Providers;

/// <summary>
/// The default, zero-configuration provider: deterministic, no network call, no API key. This is
/// what makes the feature usable out of the box for every Lyra install rather than only for
/// tenants who've paid for an OpenAI/Anthropic key — real providers are an upgrade, not a
/// requirement. It builds a real hero + features + pull-quote page from the prompt's own words.
/// </summary>
public sealed partial class MockAiProvider : IAiProvider
{
    public string Name => "Mock";

    public Task<PageGenerationPlan> GeneratePageAsync(PageGenerationRequest request, CancellationToken ct = default)
    {
        var subject = ExtractSubject(request.Prompt);
        var title = string.IsNullOrEmpty(subject) ? "New page" : Capitalize(subject);
        var highlights = ExtractHighlights(request.Prompt);

        var widgets = new List<WidgetBlock>();
        var textBlockType = PreferredTextWidgetType(request.AvailableWidgetTypes);

        if (textBlockType is not null)
        {
            widgets.Add(new WidgetBlock
            {
                ContentType = textBlockType,
                Html = $"""
                    <div class="hero">
                        <p class="eyebrow">Welcome</p>
                        <h1>{title}</h1>
                        <p class="lede">{BuildIntro(title, request.Prompt)}</p>
                    </div>
                    """,
            });

            if (highlights.Count > 0)
            {
                var features = string.Join("\n", highlights.Select(h => $"""
                    <div class="feature">
                        <h3>{h}</h3>
                        <p>Learn more about {h.ToLowerInvariant()}.</p>
                    </div>
                    """));

                widgets.Add(new WidgetBlock
                {
                    ContentType = textBlockType,
                    Html = $"""<div class="feature-grid">{features}</div>""",
                });
            }
        }

        // Deliberately NOT targeting the "Blockquote" content type here: unlike Paragraph/RawHtml
        // (both a part named after the type holding a single Content.Html field — confirmed via
        // the admin form), Blockquote's part holds a plain TextField ("Quote.Text", no HTML
        // wrapping), so ContentPlanApplier's generic Content.Html setter silently produces an
        // empty widget for it. A real `<blockquote>` tag inside a Paragraph/RawHtml block gets the
        // same pull-quote visual via theme.css without that mismatch.
        if (textBlockType is not null)
        {
            widgets.Add(new WidgetBlock
            {
                ContentType = textBlockType,
                Html = $"<blockquote>Everything we needed for {subject}, in one place.</blockquote>",
            });
        }

        return Task.FromResult(new PageGenerationPlan { PageTitle = title, Widgets = widgets });
    }

    /// <summary>Paragraph and RawHtml share the same single-HTML-field shape this provider targets; prefer
    /// whichever is actually enabled on the tenant, in that order.</summary>
    private static string? PreferredTextWidgetType(IReadOnlyList<string> available) =>
        available.FirstOrDefault(t => t is "Paragraph" or "RawHtml");

    private static string ExtractSubject(string prompt)
    {
        var match = SubjectPattern().Match(prompt);
        if (match.Success) return match.Groups[1].Value.Trim().TrimEnd('.', '!', '?');

        // Fall back to the prompt itself, trimmed to a reasonable page-title length.
        var trimmed = prompt.Trim();
        return trimmed.Length <= 60 ? trimmed : string.Concat(trimmed.AsSpan(0, 57), "...");
    }

    private static List<string> ExtractHighlights(string prompt)
    {
        var afterWith = WithPattern().Match(prompt);
        var source = afterWith.Success ? afterWith.Groups[1].Value : prompt;

        var parts = source
            .Split([",", " and "], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => p.Length is > 2 and < 40)
            .Select(Capitalize)
            .Distinct()
            .Take(3)
            .ToList();

        return parts;
    }

    private static string BuildIntro(string title, string prompt) =>
        $"{title} — built from your own description: \"{prompt.Trim()}\"";

    private static string Capitalize(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];

    [GeneratedRegex(@"(?:a|an)\s+(?:landing\s+)?page\s+for\s+(?:a|an)\s+(.+?)(?:\s+with\b|$)", RegexOptions.IgnoreCase)]
    private static partial Regex SubjectPattern();

    [GeneratedRegex(@"\bwith\s+(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex WithPattern();
}
