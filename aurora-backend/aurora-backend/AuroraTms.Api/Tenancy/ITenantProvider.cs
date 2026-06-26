namespace AuroraTms.Api.Tenancy;

/// <summary>
/// Holds the tenant resolved for the current request (scoped per request).
/// Set by TenantResolutionMiddleware, read by AppDbContext's query filters and
/// by controllers when stamping new rows.
/// </summary>
public interface ITenantProvider
{
    /// <summary>The current tenant's id (e.g. "CUS-0001"), or null if unresolved.</summary>
    string? TenantId { get; set; }
}

public class TenantProvider : ITenantProvider
{
    public string? TenantId { get; set; }
}
