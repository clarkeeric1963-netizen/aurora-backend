using AuroraTms.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Tenancy;

/// <summary>
/// Resolves which tenant a request belongs to, in this order:
///   1. The "X-Tenant" header (dev/testing convenience — lets you test tenant
///      isolation before you own a wildcard domain).
///   2. The left-most label of the Host header, treated as the tenant's subdomain
///      (e.g. "greenfield" in "greenfield.yourcompany.com").
///
/// The candidate is matched against Customers.Subdomain. If it matches, the
/// canonical tenant id is stored on ITenantProvider and every DB query is then
/// auto-scoped to that tenant.
///
/// Paths that are NOT tenant-scoped (health checks, swagger, root, and the
/// platform "/api/customers" registry) are allowed through without a tenant.
/// Any other "/api/..." request with no resolvable tenant is rejected, so data
/// can never be served without a tenant in scope.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    // Hosts that never carry a tenant subdomain (local dev + Railway's default domain).
    private static readonly string[] NonTenantHostSuffixes = { "up.railway.app", "localhost", "127.0.0.1" };

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx, AppDbContext db, ITenantProvider tenant)
    {
        var path = ctx.Request.Path.Value ?? "";

        // Only data routes are tenant-gated. The UI, static assets, health,
        // swagger, root, and the platform "/api/customers" registry don't need a tenant.
        bool isApi = path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
        bool isCustomersRegistry = path.StartsWith("/api/customers", StringComparison.OrdinalIgnoreCase);
        bool tenantOptional = !isApi || isCustomersRegistry;

        // 1. Figure out the candidate tenant slug.
        string? candidate = ctx.Request.Headers["X-Tenant"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(candidate))
        {
            var host = ctx.Request.Host.Host; // no port
            bool isNonTenantHost = NonTenantHostSuffixes.Any(s =>
                host.Equals(s, StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith("." + s, StringComparison.OrdinalIgnoreCase));

            if (!isNonTenantHost)
            {
                var firstLabel = host.Split('.').FirstOrDefault();
                // Ignore a bare apex like "yourcompany.com" or a "www" label.
                if (!string.IsNullOrWhiteSpace(firstLabel) &&
                    !firstLabel.Equals("www", StringComparison.OrdinalIgnoreCase) &&
                    host.Contains('.'))
                {
                    candidate = firstLabel;
                }
            }
        }

        // 2. Resolve the candidate to a real tenant.
        if (!string.IsNullOrWhiteSpace(candidate))
        {
            var slug = candidate.Trim().ToLowerInvariant();
            var customer = await db.Customers
                .IgnoreQueryFilters() // the registry isn't tenant-filtered
                .FirstOrDefaultAsync(c => c.Subdomain == slug);

            if (customer is not null)
                tenant.TenantId = customer.Id;
            else if (!tenantOptional)
            {
                ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                await ctx.Response.WriteAsJsonAsync(new { error = $"Unknown tenant '{slug}'." });
                return;
            }
        }

        // 3. Enforce: tenant-scoped routes must have a tenant.
        if (!tenantOptional && string.IsNullOrWhiteSpace(tenant.TenantId))
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(new
            {
                error = "No tenant in request. Use a tenant subdomain (e.g. greenfield.yourcompany.com) " +
                        "or, for testing, send an 'X-Tenant: <subdomain>' header."
            });
            return;
        }

        await _next(ctx);
    }
}
