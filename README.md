# Lyra CMS

An open-source, multi-tenant CMS built on [Orchard Core](https://orchardcore.net/) — simpler to
run than WordPress or Shopify, but built to handle what they handle: content, themes, and
installable modules that bring their own admin-side management screens.

[![Build](https://github.com/marvinok26/Lyra-CMS/actions/workflows/build.yml/badge.svg)](https://github.com/marvinok26/Lyra-CMS/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## What makes Lyra different

- **Multi-tenant from day one.** One deployment hosts many independent sites, each with its own
  database, content, theme, and enabled modules — provisioned by hand from the platform admin, or
  self-service via a public `/signup` form that hands a working store back in one request.
- **AI-assisted page building.** Describe a page in plain language; Lyra proposes a layout using
  the widgets the tenant actually has installed, as an editable draft reviewed before publishing.
  The AI backend is pluggable — a zero-config mock provider, OpenAI, or Anthropic — never locked
  to one vendor.
- **Modules bring their own admin.** Installing a module can add a whole admin section (Commerce →
  Products, with price and stock at a glance) without touching core code, using Orchard Core's
  native extensibility.
- **Simple where it counts.** The public-facing side of any site built on Lyra stays clean and
  fast — server-rendered Liquid/Razor, no separate frontend build — while complexity lives in the
  admin, where it belongs.

## What's built

| Piece | What it does |
|---|---|
| **Multi-tenancy** | Isolated tenants (own database, content, theme, modules), created from the platform admin or self-service. See `docs/multi-tenancy.md`. |
| **`Lyra.PublicTheme`** | The default storefront theme — server-rendered Liquid, warm editorial design. |
| **`Lyra.AiPageBuilder`** | Prompt → real page, scoped to the tenant's actual widget catalog, created as a draft. See `docs/ai-page-generator.md`. |
| **`Lyra.Commerce`** | Product content type, a Commerce → Products admin screen, and a live storefront widget — the reference example of a module bringing its own admin CRUD. See `docs/commerce.md`. |
| **`Lyra.Onboarding`** | Public self-service tenant signup (`/signup`) — one form, a fully provisioned store. See `docs/onboarding.md`. |

## Running locally

```bash
docker compose up --build
```

Open `http://localhost:5010` — the Default tenant is set up automatically (admin / see
`docker-compose.yml` for the default credentials, override via `.env`, see `.env.example`).

From there:
- **Platform admin** (`/Admin`): create tenants by hand under Multi-Tenancy → Tenants.
- **Self-service** (`/signup`): anyone can spin up their own store — enable the
  `Lyra.Onboarding` feature once on the Default tenant first (see `docs/onboarding.md`).

## Architecture

```
src/
├── Lyra.Cms.Web/     the Orchard Core host — composition, not logic
├── Modules/            Lyra.AiPageBuilder, Lyra.Commerce, Lyra.Onboarding
└── Themes/              Lyra.PublicTheme
```

See `docs/architecture.md` for how the pieces fit together and why this is built on Orchard Core
rather than from scratch.

## Docs

- `docs/architecture.md` — the big picture
- `docs/multi-tenancy.md` — creating and isolating tenants
- `docs/ai-page-generator.md` — how a prompt becomes a page
- `docs/commerce.md` — the Commerce module, and gotchas found building it
- `docs/onboarding.md` — self-service signup internals
- `docs/adding-a-module.md` — write your own module, with confirmed conventions and real gotchas
- `docs/adding-a-theme.md` — write your own theme

## Contributing

See `CONTRIBUTING.md`.

## License

MIT — see `LICENSE`.
