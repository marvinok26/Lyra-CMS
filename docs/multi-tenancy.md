# Multi-tenancy

Each tenant is an isolated site: its own database, its own content, its own active theme and
enabled modules. This is proven end-to-end with two tenants (`acme`, `northwind`) sharing one
`Lyra.PublicTheme` install but rendering completely independent homepages.

## Creating a tenant

1. Pre-create its database on the shared SQL Server instance (Orchard Core's SQL Server provider
   expects the database to already exist — it won't create it for you):
   ```sql
   CREATE DATABASE LyraAcme
   ```
2. As the platform admin (the Default tenant), go to **Multi-Tenancy → Tenants → Add Tenant**.
   Fill in:
   - **Name**: an internal identifier, e.g. `Acme`
   - **Request URL Prefix**: the path segment the tenant is served under, e.g. `acme` → `/acme`
   - **Recipe**: `Blog` gives you Page/BlogPost content types, Widgets, and Layers out of the box —
     a good starting point for a real site (the `SaaS` recipe used by the Default tenant is
     deliberately minimal, since its job is hosting tenants, not being one)
   - **Database Provider**: `Sql Server` (shown in the form as `SqlConnection`)
   - **Connection String**: pointing at the database you just created
3. Save. The new tenant appears with status **Uninitialized** and a one-time **Setup** link.
4. Follow the Setup link. This is the tenant's own first-run wizard — site name, timezone, and its
   own admin account (completely separate from the Default tenant's admin).

## Setting the tenant's theme

Log in to the tenant's own `/Admin` (not the Default tenant's) and go to **Design → Themes**,
then activate `Lyra.PublicTheme`. Each tenant chooses its own theme independently.

## Proof of isolation

With `acme` and `northwind` both running Lyra.PublicTheme:

- `http://localhost:5010/acme/` and `http://localhost:5010/northwind/` render different homepage
  content, authored independently through each tenant's own admin.
- Each tenant's content lives in its own database (`LyraAcme`, `LyraNorthwind`) — there is no
  shared-table-with-a-tenant-column here to accidentally leak across tenants; isolation is enforced
  at the connection level.
- Widgets attached via one tenant's **Design → Widgets/Layers** (e.g. a link in the Header zone)
  only ever render on that tenant's site.
