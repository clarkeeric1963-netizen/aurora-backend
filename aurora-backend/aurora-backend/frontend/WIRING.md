# Wiring the Aurora TMS frontend to the live API

The current `Aurora_TMS_v4_0.html` keeps all data in memory in seed arrays like
`ordersDataInit`, `driversData`, `accountsDataInit`, `terminalsData`,
`customersInit`, and `companyAdmins`. To make it "live", you replace those seed
arrays with data fetched from the .NET API.

This is **incremental** — you can wire one screen at a time, and unwired screens
keep working off their seed data. Nothing breaks while you migrate.

## Step 1 — load the client

In `Aurora_TMS_v4_0.html`, just before the closing `</head>` (or above the main
`<script>`), add:

```html
<script>window.AURORA_API_BASE = "https://YOUR-SERVICE.up.railway.app";</script>
<script src="aurora-api.js"></script>
```

(For local testing against docker-compose, set it to `http://localhost:8080`.)

## Step 2 — replace a seed array with live data

The app's state is created with `useState(...)`. The pattern is: start empty,
then load from the API in a `useEffect`.

Find where orders state is initialized (it uses `ordersDataInit`) and change the
initialization + add a load effect. Conceptually:

```js
// BEFORE
const [orders, setOrders] = useState(ordersDataInit);

// AFTER
const [orders, setOrders] = useState([]);
useEffect(() => {
  AuroraAPI.orders.list()
    .then(setOrders)
    .catch(err => console.error("orders load failed", err));
}, []);
```

The API returns the same field names the UI already uses (camelCase: `id`,
`customer`, `origin`, `dest`, `status`, `lineItems`, `shipper`, etc.), so the
rest of the rendering code keeps working.

## Step 3 — make edits persist

Wherever the UI mutates an order in state (status change, edit save), also call
the API so the change survives a refresh:

```js
async function saveOrder(updated) {
  await AuroraAPI.orders.update(updated.id, updated);
  setOrders(prev => prev.map(o => o.id === updated.id ? updated : o));
}

async function createOrder(draft) {
  const created = await AuroraAPI.orders.create(draft);   // server assigns id if omitted
  setOrders(prev => [created, ...prev]);
}

async function deleteOrder(id) {
  await AuroraAPI.orders.remove(id);
  setOrders(prev => prev.filter(o => o.id !== id));
}
```

## Available resources

| UI seed array      | API client call            | Endpoint           |
|--------------------|----------------------------|--------------------|
| `ordersDataInit`   | `AuroraAPI.orders`         | `/api/orders`      |
| `driversData`      | `AuroraAPI.drivers`        | `/api/drivers`     |
| `accountsDataInit` | `AuroraAPI.accounts`       | `/api/accounts`    |
| `terminalsData`    | `AuroraAPI.terminals`      | `/api/terminals`   |
| `customersInit`    | `AuroraAPI.customers`      | `/api/customers`   |
| `companyAdmins`    | `AuroraAPI.users`          | `/api/users`       |

Each has `.list(query?)`, `.get(id)`, `.create(data)`, `.update(id, data)`,
`.remove(id)`.

Filtered lists:
- `AuroraAPI.orders.list({ status: "In Transit" })`
- `AuroraAPI.orders.list({ customer: "Greenfield Logistics" })`
- `AuroraAPI.drivers.list({ terminal: "ORL" })`

## Notes

- The other ~40 domains in the app (EDI 204/214/997, rating tables, settlements,
  yard/dock ops, routes, etc.) aren't modeled in the backend yet. They follow the
  exact same pattern: add a model + DbSet + controller, then swap the seed array.
  The six above are the core that everything else references.
- CORS is open (`AllowAnyOrigin`) so the file can call the API from anywhere
  during development. Lock this down to your real frontend origin before going to
  production (see `Program.cs`).
