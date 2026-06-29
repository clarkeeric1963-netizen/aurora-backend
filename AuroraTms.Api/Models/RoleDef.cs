using System.Text.Json;

namespace AuroraTms.Api.Models;

/// <summary>
/// A GLOBAL role definition, owned by Aurora (the provider) and shared by every
/// Company/tenant. This is deliberately NOT tenant-scoped — it is the first
/// global table in the system. A user (tenant-scoped) is ASSIGNED one of these
/// roles by name; the role carries a default permission profile (module
/// visibility) that the user screen can apply and then fine-tune per person.
/// </summary>
public class RoleDef
{
    public string Id { get; set; } = default!;        // e.g. "ROLE-dispatcher"
    public string Name { get; set; } = default!;      // e.g. "Dispatcher"
    public string? Description { get; set; }

    // Permission profile (start simple = module visibility):
    // { "orders": true, "dispatch": true, "yard": false, ... }
    public JsonDocument? Modules { get; set; }

    public bool IsSystem { get; set; }                // seeded default vs custom
    public int SortOrder { get; set; }
}
