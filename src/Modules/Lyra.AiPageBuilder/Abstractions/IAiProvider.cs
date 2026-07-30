namespace Lyra.AiPageBuilder.Abstractions;

/// <summary>
/// One implementation per AI backend (OpenAI, Anthropic, a local Ollama model, or the built-in
/// zero-configuration Mock provider). Nothing in the rest of the module depends on a specific
/// vendor's API shape — swapping the active provider is a configuration change, not a code change.
/// </summary>
public interface IAiProvider
{
    /// <summary>Matched against the configured active-provider name (see AiPageBuilderOptions).</summary>
    string Name { get; }

    Task<PageGenerationPlan> GeneratePageAsync(PageGenerationRequest request, CancellationToken ct = default);
}
