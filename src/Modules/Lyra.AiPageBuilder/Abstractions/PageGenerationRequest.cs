namespace Lyra.AiPageBuilder.Abstractions;

/// <summary>
/// AvailableWidgetTypes is the tenant's actual, currently-enabled catalog of widget content types
/// (queried live via IContentDefinitionManager) — the provider is only ever allowed to compose the
/// page from this list, never invent a widget type that doesn't exist on this install. This is
/// what keeps generation reliable: the AI's job is layout and copy, not schema invention.
/// </summary>
public sealed class PageGenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public IReadOnlyList<string> AvailableWidgetTypes { get; set; } = [];
}
