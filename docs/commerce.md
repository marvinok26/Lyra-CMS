# Commerce (`Lyra.Commerce`)

The reference example of Lyra's core extensibility claim: a plugin brings its own admin CRUD,
not just a content type dropped into the generic editor.

## What it adds

- A **Product** content type (name, SKU, description, price, currency, category, photo, stock,
  track-inventory) defined entirely from stock Orchard Core fields — no custom C# part needed for
  the data itself, since none of it requires code beyond what
  `TextField`/`HtmlField`/`NumericField`/`BooleanField`/`MediaField` already give you.
- A **Commerce → Products** admin screen (`ProductAdminController`) that lists every product with
  a thumbnail, category, price+currency, and stock at a glance — a view Orchard's generic content
  list doesn't give you out of the box. Create and Edit deliberately hand off to Orchard's own
  content item editor rather than reimplementing field-by-field form handling; this module owns
  the list and delete experience, which is where a domain-specific view actually helps.
- A **Product grid widget** (`ProductGridWidgetPart`, a real code-based `ContentPart` with its own
  `ContentPartDisplayDriver`) that queries the live catalog — published `Product` items, most
  recently created first, capped at an admin-configurable count, optionally filtered to a single
  category — and renders it (photo, name, SKU, price+currency) into any zone or page. Nothing is
  cached on the widget itself, so the storefront always reflects the current catalog.

## Admin routes

Module-owned MVC controllers under `[Admin]` are reachable at
`/Admin/{ModuleId}/{Controller}/{Action}` — for this module, `/Admin/Lyra.Commerce/ProductAdmin/Index`.
This is the convention for every `[Admin]` controller in this project, including `Lyra.AiPageBuilder`'s
(`/Admin/Lyra.AiPageBuilder/AiPageBuilderAdmin/Index`) — an earlier version of this doc claimed the
two modules' routes differed; re-checked directly against a running tenant during the Phase 5 docs
pass and that was wrong, not a real inconsistency. See `docs/adding-a-module.md` for the confirmed
route and view-folder conventions in one place.

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
  one — has something to attach to. The Product's photo field also needs `OrchardCore.Media`
  enabled specifically; it isn't pulled in by any of the above.
- **`MediaField.Paths` posts as a JSON array of objects, not a plain string array.** The real admin
  form's Vue widget serializes `[{"path":"...","isNew":true,"isRemoved":false,"mediaText":"",
  "anchor":{"x":0.5,"y":0.5}}]` into the hidden `Paths` input — `MediaFieldDisplayDriver.UpdateAsync`
  deserializes it into `List<EditMediaFieldItemInfo>`, so posting a plain `["path.png"]` throws a
  `JsonException` server-side (caught only by testing an actual form submission, not by reading
  the field's C# `string[]` shape, which looks like it should accept a plain array).
- **Reading a `MediaField`'s `Paths` back off `ContentItem.Content` needs an explicit cast, not a
  type pattern.** Each element in the dynamic array is a `JsonDynamicValue` wrapper, not a plain
  `string` — `path is string s` silently never matches (no error, the image URL just stays null);
  `(string?)path` works. The same dynamic-JSON-casting gotcha as `Lyra.AiPageBuilder`'s
  `ExtractHtml`, now confirmed on array elements too, not just object properties.
- **A `TextFieldPredefinedListEditorSettings` (dropdown/radio options) is silently ignored unless
  the field's `ContentPartFieldSettings.Editor` is also set to `"PredefinedList"`.**
  `TextFieldPredefinedListEditorSettingsDriver.UpdateAsync` checks that string before honoring the
  options at all; the options themselves save to the database looking completely correct either
  way, so this is only visible by checking the *rendered admin form* HTML, not the stored JSON. Set
  it via the field builder's `.WithEditor("PredefinedList")`.
- **A shipped `DataMigration` step should never be edited once a real tenant has run it.** Each
  tenant's `DataMigrationRecord` remembers the highest version number executed per migration
  class; editing an already-applied `UpdateFromNAsync` method and re-enabling the feature does
  **not** re-run it — disable/enable only re-triggers steps whose version number is higher than
  what's recorded. A late-discovered bug in an applied step needs a new `UpdateFromN+1Async`, not
  an edit to the old one (this module actually hit this: the `PredefinedList` fix above shipped as
  its own follow-up step rather than a change to the step that first added the Currency field).

## Known scope cuts

- No multi-currency conversion — `Currency` is a plain code (USD/EUR/GBP/KES) stored alongside the
  price, not converted between them; a storefront showing products in different currencies won't
  total them correctly without that logic added separately.
- Category filtering on the widget matches one exact category string (case-insensitive); it isn't
  backed by a SQL index (would need a `TextFieldIndexProvider` registration this project doesn't
  ship), so it filters in-memory over the published catalog — fine at demo scale, not at real
  catalog size. No manual per-product curation (a "pick these specific products" widget mode).
