using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lyra.AiPageBuilder.Abstractions;
using Lyra.AiPageBuilder.Settings;

namespace Lyra.AiPageBuilder.Providers;

/// <summary>
/// Talks to a local Ollama server's /api/chat endpoint (default http://localhost:11434), the
/// no-cloud-vendor option alongside OpenAI/Anthropic. Ollama's "format" request field accepts a
/// JSON Schema object directly (supported since Ollama 0.5) and constrains the model's output the
/// same way OpenAI's response_format/Anthropic's tool_choice do — same PlanJsonSchema, no
/// separate parsing path needed.
/// </summary>
public sealed class OllamaProvider(HttpClient httpClient, AiPageBuilderSettingsResolver settingsResolver) : IAiProvider
{
    private static readonly JsonSerializerOptions PlanDeserializeOptions = new() { PropertyNameCaseInsensitive = true };

    public string Name => "Ollama";

    public async Task<PageGenerationPlan> GeneratePageAsync(PageGenerationRequest request, CancellationToken ct = default)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync();

        var body = new JsonObject
        {
            ["model"] = settings.Model ?? "llama3.1",
            ["stream"] = false,
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "system",
                    ["content"] = "You generate simple web page layouts for a CMS. Only use the widget " +
                        "content types listed as available; never invent a type that isn't listed.",
                },
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = $"Prompt: {request.Prompt}\nAvailable widget types: {string.Join(", ", request.AvailableWidgetTypes)}",
                },
            },
            ["format"] = PlanJsonSchema.Build(request.AvailableWidgetTypes),
        };

        var baseUrl = settings.OllamaBaseUrl.TrimEnd('/');
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/chat")
        {
            Content = JsonContent.Create(body),
        };

        using var response = await httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Ollama returned an empty response.");

        var content = payload["message"]?["content"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Ollama response did not contain a message.");

        return JsonSerializer.Deserialize<PageGenerationPlan>(content, PlanDeserializeOptions)
            ?? throw new InvalidOperationException("Could not parse the generated page plan.");
    }
}
