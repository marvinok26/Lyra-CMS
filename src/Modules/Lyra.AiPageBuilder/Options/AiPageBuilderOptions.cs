namespace Lyra.AiPageBuilder.Options;

/// <summary>
/// Host-level configuration (appsettings/environment, not yet per-tenant admin UI — a deliberate
/// v1 scope cut, see docs/ai-page-generator.md). ActiveProvider selects which registered
/// IAiProvider.Name handles generation; ApiKey/Model are read by whichever provider needs them.
/// </summary>
public sealed class AiPageBuilderOptions
{
    public const string SectionName = "Lyra:AiPageBuilder";

    /// <summary>"Mock" (default, no configuration needed), "OpenAI", or "Anthropic".</summary>
    public string ActiveProvider { get; set; } = "Mock";
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
}
