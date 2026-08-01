# Commerce (`Lyra.Commerce`)

The reference example of Lyra's core extensibility claim: a plugin brings its own admin CRUD,
not just a content type dropped into the generic editor.

## What it adds

- A **Product** content type (name, SKU, description, price, stock, track-inventory) defined
  entirely from stock Orchard Core fields — no custom C# part needed for the data itself, since
  none of it requires code beyond what `TextField`/`HtmlField`/`NumericField`/`BooleanField`
  already give you.
- A **Commerce → Products** admin screen (`ProductAdminController`) that lists every product with
  its price and stock at a glance — a view Orchard's generic content list doesn't give you out of
  the box. Create and Edit deliberately hand off to Orchard's own content item editor rather than
  reimplementing field-by-field form handling; this module owns the list and delete experience,
  which is where a domain-specific view actually helps.
- A **Product grid widget** (`ProductGridWidgetPart`, a real code-based `ContentPart` with its own
  `ContentPartDisplayDriver`) that queries the live catalog — published `Product` items, most
  recently created first, capped at an admin-configurable count — and renders it into any zone or
  page. Nothing is cached on the widget itself, so the storefront always reflects the current
  catalog.

## Admin routes

Module-owned MVC controllers under `[Admin]` are reachable at
`/Admin/{ModuleId}/{Controller}/{Action}` — for this module,
`/Admin/Lyra.Commerce/ProductAdmin/Index` (confirmed by inspecting the actual "Commerce" nav
link's generated `href`, not assumed from the AI Page Builder module's convention, which turned
out to omit the module ID segment instead — the two modules' generated routes differ and both
were verified against the running app rather than guessed).

## Known gotchas (found by testing against the running stack, not assumed)

- **`AddDataMigration<T>()` must be called explicitly** in the module's `Startup.ConfigureServices`.
  A class named `Migrations` deriving from `DataMigration` is *not* auto-discovered by convention —
  without the explicit registration, the feature enables successfully (no error) but the content
  type is silently never created. Caught by enabling the feature, hitting a 500 ("Content Type
  Product does not exist"), and finding `DataMigrationRecord` had no entry for the module at all.
- **View models passed through `Initialize<TModel>()` (the shape factory) can't be `sealed`.**
  Orchard builds a Castle DynamicProxy of the model type; a sealed class throws
  `TypeLoadException: parent type is sealed` the moment the shape is built. This only applies to
  models used as content-part display/edit shapes — plain MVC controller view models (`return
  View(model)`) don't go through the shape factory and can stay sealed.
- **A freshly created Layer's rule is not "always true" by default.** The Layers admin form doesn't
  expose a plain "Rule" field — rules are built through a separate condition-builder UI
  (`/Admin/Layers/Rules/Create?name={layer}&type={ConditionType}`). A layer saved with an empty
  condition list matches nothing. The built-in "Always" layer that ships with a content-bearing
  recipe carries an explicit `BooleanCondition` set to `true`; a layer created by hand needs the
  same condition added before any widget assigned to it will render.
- The **`SaaS` recipe is intentionally minimal** — it does not enable `OrchardCore.Contents`,
  `OrchardCore.ContentFields`, `OrchardCore.Autoroute`, `OrchardCore.Title`, `OrchardCore.Widgets`,
  or `OrchardCore.Layers`, and it does not apply a theme. A tenant provisioned from it needs those
  features (and a theme) enabled explicitly before any content-bearing module — including this
  one — has something to attach to.

## Known scope cuts

- No product images (`MediaField`) in v1 — the CRUD and storefront-query pattern is the point of
  this module, not a full catalog feature set.
- No multi-currency: `Price` is a plain decimal, no currency field.
- The product grid widget shows the *N most recent* products; no manual curation (picking specific
  products) or category filtering yet.
