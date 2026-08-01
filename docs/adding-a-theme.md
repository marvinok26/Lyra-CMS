# Adding a theme

A worked walkthrough grounded in `Lyra.PublicTheme`, the only theme shipped so far — every piece
below is copied from (or a minimal simplification of) a file that's actually running.

## 1. Scaffold the project

```
src/Themes/<ThemeName>/
├── <ThemeName>.csproj
├── Manifest.cs
├── Views/
│   ├── _ViewImports.cshtml
│   └── Layout.liquid
└── wwwroot/css/theme.css
```

`<ThemeName>.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="OrchardCore.Theme.Targets" Version="3.0.1" />
    <PackageReference Include="OrchardCore.ContentManagement" Version="3.0.1" />
    <PackageReference Include="OrchardCore.DisplayManagement" Version="3.0.1" />
    <PackageReference Include="OrchardCore.ResourceManagement" Version="3.0.1" />
  </ItemGroup>
</Project>
```

`Manifest.cs` — note the namespace: `OrchardCore.DisplayManagement.Manifest`, not
`OrchardCore.Modules.Manifest` (that one's for the `[assembly: Module(...)]` attribute modules use):

```csharp
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "Lyra.PublicTheme",
    Author = "...",
    Website = "...",
    Version = "0.0.1",
    Description = "..."
)]
```

Add it as a `ProjectReference` from `Lyra.Cms.Web.csproj` and a `<Project>` entry in
`Lyra.Cms.slnx`.

## 2. The layout and its zones

Themes here are server-rendered Liquid, not a separate frontend app. `Views/Layout.liquid`
declares the page shell and the zones a module's widgets can be placed into:

```liquid
<!DOCTYPE html>
<html lang="{{ Culture.Name }}">
<head>
    <meta charset="utf-8">
    <title>{% page_title Site.SiteName, position: "before", separator: " — " %}</title>
    {% link rel: "stylesheet", href: "~/Lyra.PublicTheme/css/theme.css" %}
    {% resources type: "Stylesheet" %}
</head>
<body>
    <header>
        <nav>{% render_section "Header", required: false %}</nav>
    </header>
    <main>
        {% render_section "Messages", required: false %}
        {% render_body %}
    </main>
    <footer>{% render_section "Footer", required: false %}</footer>
</body>
</html>
```

- `{% render_section "ZoneName", required: false %}` renders whatever's assigned to that zone via
  **Design → Widgets** (a widget's `LayerMetadata.Zone`) — the zone name is just a string both
  sides agree on, no separate registration needed. Only zones your `Layout.liquid` actually calls
  `render_section` on can show anything — a widget assigned to a zone the layout never renders is
  silently invisible, not an error.
- `{% render_body %}` renders the current content item's own body — a `Page`'s `FlowPart` widgets,
  for instance.
- Asset references need the `{% link %}` / `{% resources %}` Liquid tags, not raw `<link href="~/...">`
  — the tilde-slash path only resolves through Orchard's own tag helpers.

`Views/_ViewImports.cshtml` (needed even though the layout itself is Liquid, for any `.cshtml`
partials):

```cshtml
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
```

## 3. Styling

Plain CSS under `wwwroot/css/`, referenced via `{% link %}` in the layout — no build step required.
If a module ships its own widget (like `Lyra.Commerce`'s product grid), its view can either lean on
CSS custom properties the theme already defines (with a `var(--accent, #2f6f5e)` fallback so it
degrades gracefully under a different theme) or ship its own minimal stylesheet.

## 4. Enabling and activating

A theme, like a module, isn't active just by being referenced:

1. **Features** → find the theme → **Enable**.
2. **Design → Themes** → **Apply** (formally: two separate POSTs — `/Admin/Themes/Enable/{id}`
   then `/Admin/Themes/SetCurrentTheme/{id}` — the admin UI's "Set as current theme" button on the
   Themes page does both).

Each tenant has its own current theme — enabling and applying is per-tenant, not global.
