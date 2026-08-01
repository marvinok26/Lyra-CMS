# Architecture

Lyra CMS is built on [Orchard Core](https://orchardcore.net/), an open-source, modular ASP.NET
Core application framework. Orchard Core provides the parts that are hardest to get right in a
multi-tenant CMS — dynamic module loading, per-module admin UI registration, theming, and tenant
isolation — so Lyra's own code focuses on what makes it distinct.

## The three moving pieces

**`src/Lyra.Cms.Web`** is the host application: an Orchard Core `Cms.Web` app referencing every
module and theme this project ships. It has almost no custom code of its own — its job is
composition, not logic.

**`src/Modules/*`** are custom modules, each a Razor Class Library referenced by the host project.
A module can define content types and parts, register its own admin menu items and controllers
(giving it a full admin-side CRUD screen without touching core code), and expose widgets that any
theme can render. Three ship with this project:

- `Lyra.Commerce` — products, prices, and inventory, with an admin CRUD screen under Commerce →
  Products, and a storefront widget any theme can place on a page. See `docs/commerce.md`.
- `Lyra.AiPageBuilder` — the AI page/layout generator. See `docs/ai-page-generator.md` for how a
  natural-language prompt becomes real Orchard Core content.
- `Lyra.Onboarding` — the public, self-service "create your store" signup at `/signup`, wrapping
  tenant creation and setup in one request. See `docs/onboarding.md`.

**`src/Themes/*`** are custom themes. `Lyra.PublicTheme` is the public-facing theme sites use by
default; it's server-rendered Razor/Liquid, not a separate frontend app — a module's widget (like
Commerce's product grid) renders correctly in it with no extra frontend work.

## Multi-tenancy

Each tenant is an isolated site: its own database, its own content, its own set of enabled modules
and active theme. Tenants are created and managed from **Multi-Tenancy → Tenants** in the admin
(see `docs/multi-tenancy.md`), or self-service via `/signup` (`Lyra.Onboarding`, see
`docs/onboarding.md`), which wraps the same underlying tenant-creation and setup operations in a
single public request.

## Why build on Orchard Core instead of from scratch

The differentiators here — AI-assisted page generation, a commerce module with its own admin, a
clean multi-tenant story — are what's worth building custom. Module loading, admin extensibility,
and tenant isolation are solved problems; re-solving them would be months of infrastructure work
before any of the actual product could exist. Building on Orchard Core means that work goes
straight into the parts that make Lyra worth using.
