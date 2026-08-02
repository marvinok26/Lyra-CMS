namespace Lyra.AiPageBuilder.Options;

/// <summary>
/// Host-level defaults (appsettings/environment). Any tenant can override ActiveProvider/ApiKey/
/// Model/OllamaBaseUrl from its own admin (Settings → AI Page Builder) — see
/// AiPageBuilderSettingsResolver, which merges the two, tenant taking precedence field-by-field.
/// </summary>
public sealed class AiPageBuilderOptions
{
    public const string SectionName = "Lyra:AiPageBuilder";

    /// <summary>"Mock" (default, no configuration needed), "OpenAI", "Anthropic", or "Ollama".</summary>
    public string ActiveProvider { get; set; } = "Mock";
    public string? ApiKey { get; set; }
    public string? Model { get; set; }

    /// <summary>Base URL of a local Ollama server. Defaults to the standard local install address.</summary>
    public string? OllamaBaseUrl { get; set; } = "http://localhost:11434";
}
