using System.Text.Json;

namespace AuroraTms.Api.Models;

/// <summary>
/// A staff/admin user (maps to the app's `companyAdmins` and terminal-user records).
/// </summary>
public class AppUser
{
    public string Id { get; set; } = default!;          // e.g. "ADM-001"

    /// <summary>Owning tenant (Customers.Id). Auto-set on create; enforced on every query.</summary>
    public string TenantId { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = default!;
    public string? Role { get; set; }                   // Company Admin | Dispatcher | ...
    public string? Dept { get; set; }
    public bool IsAdmin { get; set; }
    public string? Terminal { get; set; }
    public string? Initials { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "Active";
    public string? InviteStatus { get; set; }

    /// <summary>Granular permissions (e.g. ["Full Access","Admin"]) — jsonb.</summary>
    public JsonDocument? Permissions { get; set; }

    /// <summary>Module access list — jsonb.</summary>
    public JsonDocument? Modules { get; set; }

    /// <summary>Terminal access scope (e.g. ["ALL"] or codes) — jsonb.</summary>
    public JsonDocument? TerminalAccess { get; set; }
}
