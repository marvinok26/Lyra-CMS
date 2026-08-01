# Contributing to Lyra CMS

Thanks for considering a contribution. Lyra is built on [Orchard Core](https://orchardcore.net/),
so a lot of "how do I..." questions are answered by Orchard Core's own docs
(https://docs.orchardcore.net/) — this file covers what's specific to Lyra.

## Running locally

```bash
docker compose up --build
```

The CMS is available at `http://localhost:5010`. The Default tenant is provisioned automatically
on first boot via Orchard Core AutoSetup (see `docker-compose.yml` for the admin credentials, or
override them via `.env` — copy `.env.example` to start). SQL Server runs as the `sqlserver`
service in the same stack; nothing else to install.

## Project layout

```
src/
├── Lyra.Cms.Web/      the Orchard Core host application
├── Modules/
│   ├── Lyra.AiPageBuilder/   prompt-to-page generation
│   ├── Lyra.Commerce/         products, admin CRUD, storefront widget
│   └── Lyra.Onboarding/       public self-service tenant signup
└── Themes/
    └── Lyra.PublicTheme/     the default storefront theme
```

See `docs/adding-a-module.md` and `docs/adding-a-theme.md` for step-by-step guides to extending
either — both are grounded in the actual modules/theme above, with the real gotchas we hit
building them, not generic Orchard Core boilerplate.

## Branching

- `main` — stable, deployable.
- `development` — active work happens here; merge into `main` when a milestone is stable.

## Coding conventions

- Follow the Display Driver / Editor-shape pattern Orchard Core uses throughout core modules for
  any new content parts — `Lyra.Commerce`'s `ProductGridWidgetPartDisplayDriver` is a working
  example in this repo, or use an Orchard Core core module (e.g. `OrchardCore.Title`) as reference.
- Keep modules self-contained: a module should not reach into another module's internals — depend
  on its public services/content parts only.
- No secrets in source. Connection strings and API keys are environment-provided (see
  `docker-compose.yml` and `.env.example`).
- Verify against the running stack, not just a successful build. Every module in this project was
  checked by actually enabling the feature, exercising the admin UI, and confirming the public
  side rendered correctly in Docker — several real bugs (documented in each module's `docs/*.md`)
  only showed up that way, not at compile time.

## CI

`.github/workflows/build.yml` runs `dotnet build` and a Docker image build on every push/PR to
`main` or `development`. There's no automated test suite yet — Orchard Core's own module/feature
system makes most of what would need testing only observable against a running shell (see the
verification note above); a CI job that spins up the full Docker Compose stack and exercises it
would be a good next contribution.

## Pull requests

Describe what changed and why. Keep PRs scoped to one module/theme/concern where possible.
