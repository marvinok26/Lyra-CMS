# Contributing to Lyra CMS

Thanks for considering a contribution. Lyra is built on [Orchard Core](https://orchardcore.net/),
so a lot of "how do I..." questions are answered by Orchard Core's own docs
(https://docs.orchardcore.net/) — this file covers what's specific to Lyra.

## Running locally

```bash
docker compose up --build
```

The CMS is available at `http://localhost:5010`. First run walks you through Orchard Core's setup
wizard (site name, admin account, and a database — Lyra defaults to SQL Server, already running as
the `sqlserver` service in `docker-compose.yml`).

## Project layout

```
src/
├── Lyra.Cms.Web/      the Orchard Core host application
├── Modules/            custom modules (e.g. Lyra.Commerce, Lyra.AiPageBuilder)
└── Themes/              custom themes (e.g. Lyra.PublicTheme)
```

See `docs/adding-a-module.md` and `docs/adding-a-theme.md` for step-by-step guides to extending
either.

## Branching

- `main` — stable, deployable.
- `development` — active work happens here; merge into `main` when a milestone is stable.

## Coding conventions

- Follow the Display Driver / Editor-shape pattern Orchard Core uses throughout core modules for
  any new content parts — see an existing module under `src/Modules/` for the shape once one
  exists, or an Orchard Core core module (e.g. `OrchardCore.Title`) as a reference.
- Keep modules self-contained: a module should not reach into another module's internals — depend
  on its public services/content parts only.
- No secrets in source. Connection strings and API keys are environment-provided (see
  `docker-compose.yml` and `.env.example`).

## Pull requests

Describe what changed and why. Keep PRs scoped to one module/theme/concern where possible.
