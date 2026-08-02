# AI Page Generator (`Lyra.AiPageBuilder`)

Turns a plain-language prompt into a real page — as an editable draft you review before it goes
live, never auto-published.

## How it works

1. **`PageGenerationOrchestrator`** queries this tenant's actual widget catalog via
   `IContentDefinitionManager.ListWidgetTypeDefinitionsAsync()` — never a fixed list baked into the
   module. If the Commerce module isn't enabled on this tenant, its product-grid widget simply
   isn't offered as an option; enable it, and it becomes available immediately.
2. That catalog is passed to the active **`IAiProvider`** as the *only* vocabulary it's allowed to
   compose the page from. The provider's job is layout and copy — never inventing a widget type
   that doesn't exist on this install.
3. The provider returns a **`PageGenerationPlan`**: a page title plus an ordered list of
   `WidgetBlock`s, each naming one of the allowed widget content types and the HTML to put in it.
   The orchestrator drops any block the provider still managed to name outside the allowed catalog,
   rather than failing the whole generation over one bad block.
4. **`ContentPlanApplier`** turns that plan into real Orchard Core content: a `Page` content item
   whose `FlowPart` embeds one widget content item per block, created as **Draft**
   (`VersionOptions.Draft`) — never published automatically.
5. The admin previews the generated blocks, then either **Publish**s the page (which — confirmed
   by testing, not assumed — cascades publish to every embedded `FlowPart` widget too) or
   **Discard**s it and tries a different prompt.

## Providers

| Provider | Configuration needed | Notes |
|---|---|---|
| `Mock` (default) | None | Deterministic, no network call — extracts a subject and up to three highlights from the prompt's own words. This is what makes the feature usable on every install with zero setup; it's a real generator, not a stub. |
| `OpenAI` | API key, optionally a model (default `gpt-4o-mini`) | Chat Completions API with Structured Outputs (`response_format: json_schema`, strict mode) — the model's response is guaranteed to match `PageGenerationPlan`'s schema. |
| `Anthropic` | API key, optionally a model | Messages API with a forced tool call (`tool_choice: {type: "tool", name: "return_page_plan"}`) — same structured-output guarantee via Anthropic's mechanism. |
| `Ollama` | A running local server (default `http://localhost:11434`), optionally a model (default `llama3.1`) | POSTs to `/api/chat` with `format` set to the same JSON Schema the other providers use — Ollama (0.5+) enforces it the same way OpenAI's `response_format` and Anthropic's `tool_choice` do. No API key, no cloud vendor. |

## Configuration: host defaults + per-tenant overrides

Host-level defaults live in `Lyra:AiPageBuilder:{ActiveProvider,ApiKey,Model,OllamaBaseUrl}`
(environment/appsettings — see `docker-compose.yml`). Any tenant can override some or all of these
from its own admin: **Settings → AI Page Builder**. `AiPageBuilderSettingsResolver` merges the two
field-by-field — a tenant only needs to set what it wants to change (e.g. run `Ollama` locally
while the host default stays `OpenAI` for other tenants); an unset field falls back to the host
value, and an unset host value falls back to `Mock`.

This replaced the original v1 host-only design (see the "Known scope cuts" note that used to be
here) — the resolver is what every provider (`OpenAiProvider`, `AnthropicProvider`,
`OllamaProvider`) actually calls now, not `IOptions<AiPageBuilderOptions>` directly.

## Why WidgetBlock is just `{ ContentType, Html }`, not a full zone/layout schema

The two stock widget types this provider targets — `Paragraph` and `RawHtml` — share the same
shape: a part named after the content type itself, holding a single HTML field (`Content.Html`).
One generic block covers a real hero + features + pull-quote page (a `<blockquote>` tag inside a
Paragraph block, styled by theme.css) without needing per-widget-type field mapping. `Blockquote`
itself is deliberately *not* targeted — its part holds a plain `TextField` (`Quote.Text`, no HTML),
not the same shape, so the generic HTML setter would silently produce an empty widget for it (this
was caught by testing, not assumed — see MockAiProvider's comment). A future widget with a richer
shape (e.g. a product grid with structured fields, not just
HTML) is a natural place to extend `WidgetBlock`, not a redesign of it.

## Known scope cuts (v1)

- **Single ordered widget list, not zones.** The page's own `FlowPart` is one ordered list; there's
  no "put this in the sidebar vs. the main column" distinction yet. For a single generated page's
  own body content this has been sufficient; site-wide furniture (header/footer widgets) still goes
  through Orchard Core's ordinary Layers/Widgets system, unaffected by this module.

## Per-tenant settings screen: implementation notes

`AiPageBuilderSettingsDisplayDriver` extends `SiteDisplayDriver<AiPageBuilderSettings>` — Orchard's
standard pattern for a per-tenant admin settings page (the same one Google Analytics/Tag Manager
use in Orchard Core itself): settings live on `ISite` (via `ISite.TryGet<T>()`/`GetOrCreate<T>()`,
the same JSON-properties-bag mechanism content parts use, just scoped to the tenant's site
settings), and the admin form is reached at `/Admin/Settings/{GroupId}` — for this module,
`/Admin/Settings/Lyra.AiPageBuilder`, wired up via an `AdminMenu` entry pointing at
`Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "Lyra.AiPageBuilder" })`.

`ISite`/`ISiteService` live in the `OrchardCore.Infrastructure.Abstractions` package — not an
obvious name to search for; found by grepping decompiled assemblies for the interface, since
neither `OrchardCore.Settings` nor `OrchardCore.Settings.Core` (the more obviously-named packages)
actually define it.
