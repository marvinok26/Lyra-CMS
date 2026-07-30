using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lyra.AiPageBuilder.Abstractions;
using Lyra.AiPageBuilder.Options;
using Microsoft.Extensions.Options;

namespace Lyra.AiPageBuilder.Providers;

/// <summary>
/// Uses the Chat Completions API's Structured Outputs mode (response_format: json_schema, strict)
/// so the model's response is guaranteed to match PageGenerationPlan's shape — no free-text
/// parsing, no "hope the model returned valid JSON" retry loop.
/// </summary>
public sealed class OpenAiProvider(HttpClient httpClient, IOptions<AiPageBuilderOptions> options) : IAiProvider
{
    private static readonly JsonSerializerOptions PlanDeserializeOptions = new() { PropertyNameCaseInsensitive = true };

    public string Name => "OpenAI";

    public async Task<PageGenerationPlan> GeneratePageAsync(PageGenerationRequest request, CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new InvalidOperationException("Lyra:AiPageBuilder:ApiKey is not configured for the OpenAI provider.");

        var body = new JsonObject
        {
            ["model"] = settings.Model ?? "gpt-4o-mini",
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
            ["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["name"] = "page_generation_plan",
                    ["strict"] = true,
                    ["schema"] = PlanJsonSchema.Build(request.AvailableWidgetTypes),
                },
            },
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = JsonContent.Create(body),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        using var response = await httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct)
            ?? throw new InvalidOperationException("OpenAI returned an empty response.");

        var content = payload["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
            ?? throw new InvalidOperationException("OpenAI response did not contain a message.");

        return JsonSerializer.Deserialize<PageGenerationPlan>(content, PlanDeserializeOptions)
            ?? throw new InvalidOperationException("Could not parse the generated page plan.");
    }
}
