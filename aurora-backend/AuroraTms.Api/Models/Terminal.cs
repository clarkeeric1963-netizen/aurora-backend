using System.Text.Json;

namespace AuroraTms.Api.Models;

/// <summary>
/// A physical terminal / hub (maps to the app's `terminalsData` records).
/// </summary>
public class Terminal
{
    public string Id { get; set; } = default!;          // e.g. "TRM-001"
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;        // e.g. "ORL"
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = "Active";
    public int Docks { get; set; }
    public int YardSpots { get; set; }
    public string? Timezone { get; set; }
    public string? Manager { get; set; }
    public string? OperatingHours { get; set; }
    public string? Type { get; set; }                   // Hub | Spoke | ...
    public bool IsAirport { get; set; }
    public bool IsWhse { get; set; }
    public string? Sqft { get; set; }
    public string? Region { get; set; }
    public string? DotNumber { get; set; }
    public string? Scac { get; set; }
    public string? Notes { get; set; }

    /// <summary>Dock geometry / equipment config, stored as jsonb.</summary>
    public JsonDocument? DockConfig { get; set; }

    /// <summary>Yard zone/spot config, stored as jsonb.</summary>
    public JsonDocument? YardConfig { get; set; }
}
