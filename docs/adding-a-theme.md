# Adding a theme

_Placeholder — will be expanded with a concrete walkthrough once `Lyra.PublicTheme` exists._

At a high level, a new theme under `src/Themes/<ThemeName>/`:

1. Is a project scaffolded via the Orchard Core `octheme` template (or hand-rolled), added as a
   `ProjectReference` from `Lyra.Cms.Web.csproj`.
2. Has a `Manifest.cs` with an `[assembly: Theme(...)]` attribute.
3. Declares its zones in `Views/Shared/Layout.cshtml` (or `.liquid`) — e.g. Header, Content,
   Sidebar, Footer — which any module's widgets can then be placed into via Design → Widgets.
4. Ships its own asset pipeline (Tailwind build output under `wwwroot/`).
5. Is set as a tenant's active theme from **Design → Themes**.
