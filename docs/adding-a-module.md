# Adding a module

_Placeholder — will be expanded with a concrete walkthrough once `Lyra.Commerce` exists as a
worked example._

At a high level, a new module under `src/Modules/<ModuleName>/`:

1. Is a Razor Class Library project, added as a `ProjectReference` from `Lyra.Cms.Web.csproj`.
2. Has a `Manifest.cs` with an `[assembly: Module(...)]` attribute describing its name,
   dependencies, and category.
3. Defines any content types/parts via a `Migrations` class using `IContentDefinitionManager`.
4. Registers its own admin menu entry via an `INavigationProvider` implementation, and any admin
   controllers/views under `Controllers/Admin` / `Views/Admin`.
5. Is enabled per-tenant from the **Features** admin screen once installed.
