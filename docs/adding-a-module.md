# Adding a module

A worked walkthrough based on the three modules actually shipped in this repo
(`Lyra.AiPageBuilder`, `Lyra.Commerce`, `Lyra.Onboarding`) — every convention below was confirmed
against a running tenant, not assumed from Orchard Core's docs. Where something surprised us the
first time, that's called out explicitly so it doesn't surprise you too.

## 1. Scaffold the project

```
src/Modules/<ModuleName>/
├── <ModuleName>.csproj      Microsoft.NET.Sdk.Razor, AddRazorSupportForMvc=true
├── Manifest.cs               [assembly: Module(...)]
├── Startup.cs                DI registrations
├── Migrations.cs             content type/part definitions (if any)
├── Permissions.cs            IPermissionProvider (if the module needs its own admin gate)
├── AdminMenu.cs               INavigationProvider (if the module has admin screens)
├── Controllers/Admin/
├── Views/
└── Views/_ViewImports.cshtml
```

Add it as a `ProjectReference` from `Lyra.Cms.Web.csproj` and a `<Project>` entry in
`Lyra.Cms.slnx`. `_ViewImports.cshtml` needs exactly this, copied verbatim from any existing module:

```cshtml
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
```

## 2. Defining content types

If your module needs a content type built entirely from stock fields (`TextField`, `HtmlField`,
`NumericField`, `BooleanField`, ...) — no custom C# part needed, see `Lyra.Commerce`'s `Product`
type for the pattern:

```csharp
public sealed class Migrations(IContentDefinitionManager contentDefinitionManager) : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await contentDefinitionManager.AlterPartDefinitionAsync("Product", part => part
            .WithField("Sku", f => f.OfType("TextField").WithDisplayName("SKU")));

        await contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
            .WithPart("TitlePart")
            .WithPart("Product")
            .Creatable().Listable().Draftable().Versionable());

        return 1;
    }
}
```

> **Gotcha:** a `Migrations : DataMigration` class is *not* auto-discovered. Register it explicitly
> in `Startup.ConfigureServices`:
> ```csharp
> services.AddDataMigration<Migrations>();
> ```
> Skipping this doesn't error — the feature enables successfully, but the content type is silently
> never created. If a content type "doesn't exist" right after enabling a feature that should have
> defined it, this is the first thing to check.

## 3. A code-based `ContentPart` (when stock fields aren't enough)

Needed when the part's job is more than holding static data — `Lyra.Commerce`'s
`ProductGridWidgetPart` queries the live product catalog at render time. The registration and
class shapes:

```csharp
public sealed class ProductGridWidgetPart : ContentPart
{
    public int MaxItems { get; set; } = 3;
}

public sealed class ProductGridWidgetPartDisplayDriver(ISession session)
    : ContentPartDisplayDriver<ProductGridWidgetPart>
{
    public override async Task<IDisplayResult> DisplayAsync(ProductGridWidgetPart part, BuildPartDisplayContext context)
    {
        var products = await session.Query<ContentItem, ContentItemIndex>(x =>
                x.ContentType == "Product" && x.Published)
            .Take(part.MaxItems).ListAsync();

        return Initialize<ProductGridWidgetViewModel>("ProductGridWidgetPart", model =>
            model.Products = /* project products */ []).Location("Detail", "Content");
    }
}
```

Register it in `Startup.ConfigureServices`:

```csharp
services.AddContentPart<ProductGridWidgetPart>()
    .UseDisplayDriver<ProductGridWidgetPartDisplayDriver>();
```

> **Gotcha:** any view model passed to `Initialize<TModel>(...)` (the shape factory) **cannot be
> `sealed`** — Orchard builds a Castle DynamicProxy of it, and a sealed class throws
> `TypeLoadException: parent type is sealed` the moment the shape is built. This only applies to
> shape view models; plain MVC controller view models (`return View(model)`) never go through the
> shape factory and can stay sealed.

Shape view templates are **flat**, not nested — `Views/ProductGridWidgetPart.cshtml` (display) and
`Views/ProductGridWidgetPart.Edit.cshtml` (edit), directly under `Views/`, matching the shape
type string passed to `Initialize`.

## 4. Admin screens

A module-owned admin controller:

```csharp
[Admin]
public sealed class ProductAdminController(/* ... */) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() { /* ... */ return View(model); }
}
```

**Confirmed route:** `/Admin/{ModuleId}/{ControllerName}/{Action}` — e.g.
`/Admin/Lyra.Commerce/ProductAdmin/Index`. This is consistent across every `[Admin]` controller in
this project (double-checked during the Phase 5 docs pass, after an earlier draft of this doc
claimed otherwise).

**Confirmed view location:** `Views/{ControllerName}/{Action}.cshtml` — e.g.
`Views/ProductAdmin/Index.cshtml`. *Not* nested under a `Views/Admin/` folder — that's the ASP.NET
Core MVC areas convention from a plain (non-Orchard) app, and it 404s here.

Register the admin nav entry:

```csharp
public sealed class AdminMenu(IStringLocalizer<AdminMenu> S) : INavigationProvider
{
    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            return ValueTask.CompletedTask;

        builder.Add(S["Commerce"], "3", commerce => commerce
            .Add(S["Products"], "1", products => products
                .Action("Index", "ProductAdmin", new { area = "Lyra.Commerce" })
                .Permission(Permissions.ManageProducts)
                .LocalNav()));

        return ValueTask.CompletedTask;
    }
}
```

`INavigationProvider.BuildNavigationAsync` returns `ValueTask`, not `Task`, in this Orchard Core
version — a common first compile error.

Enabling a POST-only admin action from a link (e.g. "Enable feature") requires a **POST**, not a
GET — the admin UI's own links to these actions are JS-intercepted, not plainly navigable.

## 5. Wire it up in `Startup.cs`

```csharp
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        services.AddContentPart<ProductGridWidgetPart>().UseDisplayDriver<ProductGridWidgetPartDisplayDriver>();
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) { }
}
```

## 6. Enable it

A module isn't active just because the host project references it. On each tenant that should use
it: **Features** in the admin → find the module → **Enable** (or drive it programmatically via
`IShellFeaturesManager.UpdateFeaturesAsync`, as `Lyra.Onboarding` does for new signups — see
`docs/onboarding.md`).

## Reference: content field storage shape

If you ever need to read a stock field's value off a `ContentItem` directly instead of through a
strongly-typed part (`ContentItem.Content` is `dynamic`, backed by System.Text.Json — `.GetValue<T>()`
doesn't work on it, use a plain cast instead):

| Field type | Storage shape |
|---|---|
| `TextField` | `Content[Part]["FieldName"]["Text"]` |
| `HtmlField` | `Content[Part]["FieldName"]["Html"]` |
| `NumericField` | `Content[Part]["FieldName"]["Value"]` |
| `BooleanField` | `Content[Part]["FieldName"]["Value"]` |

```csharp
dynamic part = product.Content.Product;
decimal? price = (decimal?)part.Price?.Value;
```
