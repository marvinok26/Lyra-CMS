using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lyra.AiPageBuilder.Abstractions;
using Lyra.AiPageBuilder.Options;
using Microsoft.Extensions.Options;

namespace Lyra.AiPageBuilder.Providers;

/// <summary>
/// Uses the Messages API's tool-use with a forced tool_choice, so the model must call
/// "return_page_plan" with input matching PageGenerationPlan's schema — the Anthropic equivalent
/// of OpenAI's structured-output mode, same guarantee: no free-text parsing.
/// </summary>
public sealed class AnthropicProvider(HttpClient httpClient, IOptions<AiPageBuilderOptions> options) : IAiProvider
{
    private const string ToolName = "return_page_plan";
    private static readonly JsonSerializerOptions PlanDeserializeOptions = new() { PropertyNameCaseInsensitive = true };

    public string Name => "Anthropic";

    public async Task<PageGenerationPlan> GeneratePageAsync(PageGenerationRequest request, CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new InvalidOperationException("Lyra:AiPageBuilder:ApiKey is not configured for the Anthropic provider.");

        var body = new JsonObject
        {
            ["model"] = settings.Model ?? "claude-sonnet-5",
            ["max_tokens"] = 2048,
            ["system"] = "You generate simple web page layouts for a CMS. Only use the widget content " +
                "types listed as available; never invent a type that isn't listed.",
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = $"Prompt: {request.Prompt}\nAvailable widget types: {string.Join(", ", request.AvailableWidgetTypes)}",
                },
            },
            ["tools"] = new JsonArray
            {
                new JsonObject
                {
                    ["name"] = ToolName,
                    ["description"] = "Return the generated page plan.",
                    ["input_schema"] = PlanJsonSchema.Build(request.AvailableWidgetTypes),
                },
            },
            ["tool_choice"] = new JsonObject { ["type"] = "tool", ["name"] = ToolName },
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = JsonContent.Create(body),
        };
        httpRequest.Headers.Add("x-api-key", settings.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");

        using var response = await httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Anthropic returned an empty response.");

        var toolUseBlock = payload["content"]?.AsArray()
            .FirstOrDefault(block => block?["type"]?.GetValue<string>() == "tool_use")
            ?? throw new InvalidOperationException("Anthropic response did not contain a tool_use block.");

        var input = toolUseBlock["input"]
            ?? throw new InvalidOperationException("Anthropic tool_use block had no input.");

        return JsonSerializer.Deserialize<PageGenerationPlan>(input.ToJsonString(), PlanDeserializeOptions)
            ?? throw new InvalidOperationException("Could not parse the generated page plan.");
    }
}
