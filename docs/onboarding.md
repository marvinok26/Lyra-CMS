# Self-Service Onboarding (`Lyra.Onboarding`)

A public "create your store" form at `/signup` that provisions and sets up a new tenant in one
request — no platform-admin login, no separate setup-link step. The built-in `/Admin/Tenants`
screen (used by hand in Phase 1 and Phase 3) is the right tool for an operator provisioning
tenants deliberately; this is the self-service front door for anyone who wants a store.

## Enabling it

Like every module in this project, `Lyra.Onboarding` isn't active just because it's referenced —
on a fresh Default tenant (provisioned via AutoSetup with the bare `SaaS` recipe), enable it once
via **Features → Lyra.Onboarding** in the platform admin, the same one-time step Phase 3 needed
for `Lyra.Commerce`. After that, `/signup` is live for anyone.

## How it works

1. **`SignupController`** (route `/signup`, only reachable on the Default tenant — mirrors how
   `OrchardCore.Tenants`' own `TenantApiController` gates itself to `IsDefaultShell()`) collects a
   store name, a URL slug, and admin credentials.
2. **`TenantDatabaseInitializer`** runs `CREATE DATABASE` up front, the same thing the Docker
   Compose `db-init` step does at boot — Orchard Core's SQL Server provider never creates the
   database itself (see `docs/multi-tenancy.md`).
3. The controller builds `ShellSettings` and calls `IShellHost.UpdateShellSettingsAsync` (creates
   an uninitialized tenant shell), then `ISetupService.SetupAsync` with the `SaaS` recipe and the
   submitted admin credentials — the exact two operations `OrchardCore.Tenants`' `TenantApiController`
   exposes over its REST API, called in-process instead of over HTTP.
4. **`StoreProvisioner`** then does everything the bare `SaaS` recipe deliberately leaves out
   (confirmed empty in Phase 3): enables `Contents`, `ContentFields`, `Title`, `Autoroute`, `Flows`,
   `Widgets`, `Layers`, `Lyra.PublicTheme`, `Lyra.Commerce`, and `Lyra.AiPageBuilder`; sets the
   public theme as current; defines `Page`, `Paragraph`, and `RawHtml` content types (the exact
   field shapes `Lyra.AiPageBuilder` already targets, established in Phase 2 — no re-deriving from
   scratch); and creates a working "Always" layer.
5. The response is a plain success page with the new store's admin login link — no email step.

## Known gotcha (found by testing, not assumed)

**Enabling a feature and immediately using its services in the same shell scope fails.**
`IShellFeaturesManager.UpdateFeaturesAsync` rebuilds the tenant's DI container, but only a *new*
scope sees the rebuilt one — the scope that issued the enable call still holds the old container,
so resolving e.g. `IContentDefinitionManager` right after enabling `OrchardCore.ContentTypes` in
the same `ShellScope.UsingChildScopeAsync` callback throws `No service for type ... has been
registered`. `StoreProvisioner.ProvisionAsync` fixes this by using two separate child-scope calls:
one that only enables features, then a second, fresh one for everything that depends on those
features being active.

## Known scope cuts

- No email verification or CAPTCHA — anyone can submit the form. Fine for a demo/CV project; a
  production deployment would want at least a rate limit in front of `/signup`.
- The "Subdomain" field is actually a URL path prefix in this deployment (`yourhost/your-slug`),
  not a real DNS subdomain — consistent with how every tenant in this project (`acme`, `commerce`,
  etc.) has worked so far. A production deployment fronted by a wildcard DNS record could set
  `RequestUrlHost` instead.
- No self-service plan/billing tier — every store gets the same feature set.
