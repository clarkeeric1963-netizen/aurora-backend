# Aurora TMS — .NET 8 + Postgres backend (Railway-ready)

This turns the in-memory `Aurora_TMS_v4_0.html` prototype into a real, live
application: an **ASP.NET Core 8 Web API** backed by **PostgreSQL**, packaged to
deploy on **Railway** in a few minutes.

## What's here

```
aurora-backend/
├─ AuroraTms.Api/
│  ├─ AuroraTms.Api.csproj      # net8.0 + EF Core + Npgsql + Swagger
│  ├─ Program.cs                # boot: DATABASE_URL/PORT handling, CORS, DB init
│  ├─ appsettings*.json
│  ├─ Models/                   # Customer, Terminal, Account, Driver, Order(+LineItem), AppUser
│  ├─ Data/
│  │  ├─ AppDbContext.cs        # EF mappings, jsonb columns, relationships
│  │  └─ DbSeeder.cs            # seeds real records from the app on first boot
│  └─ Controllers/              # full CRUD REST endpoints per entity
├─ Dockerfile                   # multi-stage build Railway uses
├─ railway.toml                 # build=Dockerfile, healthcheck=/health
├─ docker-compose.yml           # run Postgres + API locally to test first
└─ frontend/
   ├─ aurora-api.js             # browser client for the API
   └─ WIRING.md                 # how to point the React UI at the live API
```

## Data model (core entities)

Modeled from the app's actual seed arrays, so field names match the UI:

| Entity     | From the app's       | Endpoint          |
|------------|----------------------|-------------------|
| Customer   | `customersInit`      | `/api/customers`  |
| Terminal   | `terminalsData`      | `/api/terminals`  |
| Account    | `accountsDataInit`   | `/api/accounts`   |
| Driver     | `driversData`        | `/api/drivers`    |
| Order      | `ordersDataInit`     | `/api/orders`     |
| AppUser    | `companyAdmins`      | `/api/users`      |

Deeply nested sub-objects (shipper/consignee/billTo, dock/yard config, invoicing,
EDI sets, permissions, modules, etc.) are stored as **`jsonb`** columns, so the
rich shapes are preserved exactly and stay queryable. Order line items are a real
child table (`order_line_items`).

Each endpoint supports `GET` (list), `GET /{id}`, `POST`, `PUT /{id}`,
`DELETE /{id}`. Some support filters (`/api/orders?status=In%20Transit`,
`/api/drivers?terminal=ORL`).

> Scope note: the app has ~40 more domains (EDI 204/214/997, rating tables,
> settlements, routes, live dock/yard ops…). These six are the core everything
> else hangs off. Adding another domain = one model + one `DbSet` + one
> controller, following the existing pattern.

---

## A. Test locally first (recommended)

You need [Docker](https://docs.docker.com/get-docker/). From `aurora-backend/`:

```bash
docker compose up --build
```

Then open **http://localhost:8080/swagger** and try `GET /api/orders`. Postgres
data persists in the `pgdata` volume.

No Docker but have the [.NET 8 SDK](https://dotnet.microsoft.com/download)?
Start a local Postgres, then:

```bash
cd AuroraTms.Api
dotnet run            # uses ConnectionStrings:Default from appsettings.Development.json
```

---

## B. Deploy to Railway

You'll need a Railway account and the project pushed to a Git repo (GitHub), or
the [Railway CLI](https://docs.railway.com/guides/cli).

### 1. Create the project + database
- New Project → **+ New** → **Database** → **PostgreSQL**.
  Railway provisions it and exposes a `DATABASE_URL` variable automatically.

### 2. Add the API service
- **+ New** → **GitHub Repo** → pick this repo (root = `aurora-backend/`).
  Railway detects the `Dockerfile` and builds it. `railway.toml` sets the
  healthcheck to `/health`.

### 3. Link the database to the API
- Open the **API service → Variables → New Variable**, then add a **reference**:

  ```
  DATABASE_URL = ${{Postgres.DATABASE_URL}}
  ```

  (`Postgres` is the database service's name — match it exactly. This is a
  reference variable, not a pasted password.) `Program.cs` converts this URI into
  an Npgsql connection string at startup. Railway also injects `PORT`, which the
  app binds to automatically.

### 4. Expose the API publicly
- **API service → Settings → Networking → Generate Domain.**
  You'll get something like `https://aurora-api-production.up.railway.app`.

### 5. Verify
```bash
curl https://YOUR-SERVICE.up.railway.app/health
curl https://YOUR-SERVICE.up.railway.app/api/orders
```
Tables are created and seeded on first boot.

### CLI alternative (steps 2–4)
```bash
railway login
railway link            # select the project
railway up              # build & deploy from the Dockerfile
railway variables --set 'DATABASE_URL=${{Postgres.DATABASE_URL}}'
railway domain          # generate a public domain
```

---

## C. The frontend (bundled)

The app UI is bundled into the API at `AuroraTms.Api/wwwroot/index.html` and
served from the same place as the API. Visiting the service root loads the app;
its data calls go to `/api/...` on the same host, so on a tenant subdomain
(`sefl.auroratms.com`) the tenant is detected automatically — no config needed.

The UI ships with the app's built-in demo data. Connecting each screen to the
live database is a separate, incremental step — see `frontend/WIRING.md`. To wire
a screen, swap its seed array for an `AuroraAPI.<resource>.list()` call; the
client (`/aurora-api.js`) is already loaded and points at the same origin.

To test a specific tenant on the plain Railway URL (which has no subdomain), open
the browser console on the app and run `window.AURORA_TENANT = "sefl"` before the
screen loads, or just use `./smoke-test.sh` against the API.

---

## Production hardening (when you're past prototype)

- **Migrations instead of `EnsureCreated()`.** With the .NET SDK + EF tools:
  ```bash
  dotnet tool install --global dotnet-ef
  cd AuroraTms.Api
  dotnet ef migrations add Init
  ```
  Then in `Program.cs` swap `db.Database.EnsureCreated();` for
  `db.Database.Migrate();`. This lets you evolve the schema safely over time.
- **Lock down CORS** in `Program.cs` to your real frontend origin instead of
  `AllowAnyOrigin()`.
- **Add auth.** There's no authentication yet — add JWT/API keys before exposing
  real data. The app's users/permissions model is already captured in `AppUser`.
- **Backups & monitoring** on the Railway Postgres service.

## A note on what "live" required from me vs. you
I built and verified the project structure and code by hand, but this sandbox has
no .NET SDK, Docker, or network access to Railway — so I couldn't compile it or
push to your Railway account from here. Run step A once to confirm it builds in
your environment (it's a 1-command check), then step B goes live on your account.
If anything doesn't compile or deploy cleanly, tell me the exact error and I'll
fix it.
