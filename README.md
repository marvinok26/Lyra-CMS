# Lyra CMS

An open-source, multi-tenant CMS built on [Orchard Core](https://orchardcore.net/) — simpler to
run than WordPress or Shopify, but built to handle what they handle: content, themes, and
installable modules that bring their own admin-side management screens (an e-commerce module with
product/price CRUD, for example).

## What makes Lyra different

- **Multi-tenant from day one.** One deployment can host many independent sites, each with its own
  database, content, theme, and enabled modules.
- **AI-assisted page building.** Describe a page in plain language; Lyra proposes a layout using
  the widgets your site actually has installed, as an editable draft you review before publishing.
  The AI backend is pluggable — OpenAI, Anthropic, or a local model — never locked to one vendor.
- **Modules bring their own admin.** Installing a module can add a whole admin section (e.g.
  Commerce → Products) without touching core code, using Orchard Core's native extensibility.
- **Simple where it counts.** The public-facing side of any site built on Lyra stays clean and
  fast; complexity lives in the admin, where it belongs.

## Status

Early development. See `docs/architecture.md` for how the pieces fit together and the project's
roadmap.

## Running locally

```bash
docker compose up --build
```

Then open `http://localhost:5010` and complete the first-run setup wizard.

## Contributing

See `CONTRIBUTING.md`, and `docs/adding-a-module.md` / `docs/adding-a-theme.md` for how to extend
Lyra.

## License

MIT — see `LICENSE`.
