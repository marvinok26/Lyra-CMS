using System.Text.Json.Nodes;

namespace Lyra.AiPageBuilder.Providers;

/// <summary>
/// The JSON Schema for PageGenerationPlan, shared by every provider that supports structured/
/// tool-forced output (OpenAI's json_schema response format, Anthropic's tool input_schema).
/// Constraining "contentType" to an enum of the tenant's actual widget types is what stops the
/// model from inventing a widget that doesn't exist on this install.
/// </summary>
public static class PlanJsonSchema
{
    public static JsonObject Build(IReadOnlyList<string> availableWidgetTypes) => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["required"] = new JsonArray("pageTitle", "widgets"),
        ["properties"] = new JsonObject
        {
            ["pageTitle"] = new JsonObject { ["type"] = "string" },
            ["widgets"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("contentType", "html"),
                    ["properties"] = new JsonObject
                    {
                        ["contentType"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["enum"] = new JsonArray(availableWidgetTypes.Select(t => (JsonNode)t).ToArray()),
                        },
                        ["html"] = new JsonObject { ["type"] = "string" },
                    },
                },
            },
        },
    };
}
