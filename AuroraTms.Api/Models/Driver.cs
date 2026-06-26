using System.Text.Json;

namespace AuroraTms.Api.Models;

/// <summary>
/// A driver (maps to the app's `driversData` records).
/// </summary>
public class Driver
{
    public string Id { get; set; } = default!;          // e.g. "DRV-2001"

    /// <summary>Owning tenant (Customers.Id). Auto-set on create; enforced on every query.</summary>
    public string TenantId { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Pin { get; set; }
    public string Status { get; set; } = "Off Duty";    // On Duty | Driving | Off Duty | ...
    public string? Photo { get; set; }
    public string? PhotoColor { get; set; }
    public string? Cell { get; set; }
    public string? Email { get; set; }
    public string? CurrentLocation { get; set; }
    public string? DefaultEquip { get; set; }
    public string? AssignedEquip { get; set; }
    public string? CurrentOrder { get; set; }
    public string? CdlClass { get; set; }
    public string? CdlNumber { get; set; }
    public string? CdlState { get; set; }
    public string? CdlExpiry { get; set; }
    public string? MedCertExpiry { get; set; }
    public string? HomeTerminal { get; set; }
    public string? TerminalCode { get; set; }
    public int Capacity { get; set; }
    public int AvailableCapacity { get; set; }
    public string? HireDate { get; set; }
    public string? Dob { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? PayType { get; set; }
    public decimal PayRate { get; set; }
    public string? HosStatus { get; set; }
    public string? HosClock { get; set; }
    public int HosViolations { get; set; }
    public string? Mvr { get; set; }
    public bool TwicCard { get; set; }
    public bool Hazmat { get; set; }
    public bool IsYardJockey { get; set; }
    public int AccidentCount { get; set; }
    public string? Notes { get; set; }

    /// <summary>CDL endorsements (e.g. ["T","N"]) — jsonb.</summary>
    public JsonDocument? Endorsements { get; set; }

    /// <summary>CDL restrictions — jsonb.</summary>
    public JsonDocument? Restrictions { get; set; }
}
