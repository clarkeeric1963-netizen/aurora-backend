/*
 * Aurora TMS API client (browser, no build step).
 *
 * Usage: include this BEFORE your app's main <script> in the HTML:
 *   <script>window.AURORA_API_BASE = "https://your-service.up.railway.app";</script>
 *   <script src="aurora-api.js"></script>
 *
 * MULTI-TENANT: which tenant you act as is normally taken from the subdomain
 * (e.g. greenfield.yourcompany.com). For TESTING on the plain Railway URL (which
 * has no tenant subdomain), set a tenant slug and the client sends it as a header:
 *   <script>window.AURORA_TENANT = "sefl";</script>   // or "gulfstates", etc.
 *
 * Then in React code, replace in-memory seed arrays with live calls, e.g.:
 *   const [orders, setOrders] = useState([]);
 *   useEffect(() => { AuroraAPI.orders.list().then(setOrders); }, []);
 */
(function () {
  const BASE = (window.AURORA_API_BASE || "http://localhost:8080").replace(/\/$/, "");

  async function req(method, path, body) {
    const headers = { "Content-Type": "application/json" };
    if (window.AURORA_TENANT) headers["X-Tenant"] = window.AURORA_TENANT; // testing only
    const res = await fetch(BASE + path, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });
    if (!res.ok) {
      const txt = await res.text().catch(() => "");
      throw new Error(`${method} ${path} -> ${res.status} ${txt}`);
    }
    if (res.status === 204) return null;
    return res.json();
  }

  function resource(name) {
    return {
      list: (query) => req("GET", `/api/${name}${query ? "?" + new URLSearchParams(query) : ""}`),
      get: (id) => req("GET", `/api/${name}/${encodeURIComponent(id)}`),
      create: (data) => req("POST", `/api/${name}`, data),
      update: (id, data) => req("PUT", `/api/${name}/${encodeURIComponent(id)}`, data),
      remove: (id) => req("DELETE", `/api/${name}/${encodeURIComponent(id)}`),
    };
  }

  window.AuroraAPI = {
    base: BASE,
    health: () => req("GET", "/health"),
    customers: resource("customers"),
    terminals: resource("terminals"),
    accounts: resource("accounts"),
    drivers: resource("drivers"),
    orders: resource("orders"),
    users: resource("users"),
  };
})();
