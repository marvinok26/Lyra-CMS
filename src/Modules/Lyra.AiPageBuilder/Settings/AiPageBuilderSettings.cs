namespace Lyra.AiPageBuilder.Settings;

/// <summary>
/// Per-tenant override of the host-level AiPageBuilderOptions (see docs/ai-page-generator.md's
/// "known scope cuts" — this used to be host-only). Stored on ISite via TryGet/GetOrCreate, the
/// same JSON-properties-bag mechanism content parts use, just scoped to the tenant's site
/// settings instead of a content item. Every field is nullable/empty-by-default: an unset tenant
/// falls back to the host's AiPageBuilderOptions untouched (see AiPageBuilderSettingsResolver).
/// </summary>
public sealed class AiPageBuilderSettings
{
    public string? ActiveProvider { get; set; }
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
    public string? OllamaBaseUrl { get; set; }
}
