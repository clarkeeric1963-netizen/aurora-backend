# Multi-tenancy

This backend is multi-tenant: many customer companies ("tenants") share one
database and one running app, but **each tenant can only ever see its own data.**

## How it works

1. **Tenant registry.** The `customers` table is the list of tenants. Each row
   has a `subdomain` (e.g. `sefl`, `gulfstates`, `apex`).
2. **Tagged data.** Every core table (`orders`, `drivers`, `accounts`,
   `terminals`, `users`) has a `tenant_id` column pointing at a customer.
3. **Resolution.** On every request, middleware figures out the tenant — from the
   subdomain (`greenfield.yourcompany.com` → `greenfield`) or, for testing, from
   an `X-Tenant` header.
4. **Isolation.** EF Core "global query filters" automatically add
   `WHERE tenant_id = <current tenant>` to *every* query. New rows are stamped
   with the current tenant automatically, and a request can't reassign a row to
   another tenant. This is enforced in one place so a future bug can't bypass it.

Platform-level routes (`/health`, `/swagger`, `/`, and the `/api/customers`
registry) don't require a tenant. Every other `/api/...` route does — a request
with no resolvable tenant is rejected, so data is never served "untenanted."

## Seeded tenants

| Subdomain    | Company                      | Has data            |
|--------------|------------------------------|---------------------|
| `sefl`       | Southeastern Freight Lines   | Orlando terminal, Marcus Johnson, Greenfield account, 1 order |
| `gulfstates` | Gulf States Logistics        | Houston terminal, Carlos Mendez, 1 order |
| `apex`       | Apex Regional Transport      | (registry only, no ops data yet) |

## Testing isolation BEFORE you own a domain

On the plain Railway URL there's no tenant subdomain, so send the tenant as a
header. Example:

```bash
curl -H "X-Tenant: sefl"       https://YOUR.up.railway.app/api/orders   # SEFL's order
curl -H "X-Tenant: gulfstates" https://YOUR.up.railway.app/api/orders   # Gulf States' order
```

The two should return different orders — that's isolation working. The included
`./smoke-test.sh` does this check for you.

From the frontend, set `window.AURORA_TENANT = "sefl"` and `aurora-api.js` adds
the header for you.

## Going live with real tenant subdomains (when ready)

This needs a **domain you own** (~$10–15/yr). The Railway `*.up.railway.app`
address can't do wildcards.

1. Buy a domain (Railway can sell you one under the project's Settings → Domains,
   or use Namecheap/Cloudflare/etc.).
2. In Railway: API service → **Settings → Networking → Custom Domain**, enter
   `*.yourcompany.com` (the wildcard). Railway shows you DNS records to add
   (a CNAME for the wildcard and a `_acme-challenge` record).
3. Add those records at your DNS provider. Railway then issues a wildcard TLS
   certificate automatically. (If you use Cloudflare, keep the `_acme-challenge`
   record set to "DNS only," not proxied — that trips up a lot of people.)
4. Give each tenant a subdomain that matches its `customers.subdomain` value.
   Now `sefl.yourcompany.com/api/orders` returns SEFL's orders with no header
   needed.

## Onboarding a new tenant

Create a customer (tenant) with a unique subdomain, then add its data:

```bash
# 1. create the tenant
curl -X POST https://YOUR.up.railway.app/api/customers \
  -H "Content-Type: application/json" \
  -d '{"company":"New Carrier Co","subdomain":"newco","tier":"pro","status":"Active"}'

# 2. add data AS that tenant (header now, subdomain once DNS is set)
curl -X POST https://YOUR.up.railway.app/api/drivers \
  -H "Content-Type: application/json" -H "X-Tenant: newco" \
  -d '{"name":"Jane Doe","status":"Available"}'
```

## Not done yet (deliberately)

- **User login / authentication.** Subdomain identifies the *tenant*, not the
  *individual user*. For real use you'll want per-user login (so different staff
  at the same tenant get different access). The `users` table and permissions are
  modeled and ready for it.
- **Protecting `/api/customers`.** The tenant registry is currently open. Before
  production it should require platform-admin auth, since it lists all tenants.
