using System.Text.Json;

namespace AuroraTms.Api.Models;

/// <summary>
/// A tenant/company in the platform (maps to the app's `customersInit` records).
/// </summary>
public class Customer
{
    public string Id { get; set; } = default!;          // e.g. "CUS-0001"
    public string Company { get; set; } = default!;

    /// <summary>The tenant's subdomain slug, e.g. "greenfield" for greenfield.yourcompany.com. Unique.</summary>
    public string? Subdomain { get; set; }
    public string? Contact { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Tier { get; set; }                   // enterprise | pro-plus | pro | starter
    public string Status { get; set; } = "Active";
    public int Terminals { get; set; }
    public int Users { get; set; }
    public int Drivers { get; set; }
    public int OrdersMonth { get; set; }
    public string? Domain { get; set; }
    public string? Industry { get; set; }
    public string? Region { get; set; }
    public decimal? Discount { get; set; }
    public decimal Mrr { get; set; }
    public string? Notes { get; set; }
    public bool SetupComplete { get; set; }
    public DateOnly? CreatedAt { get; set; }

    /// <summary>Enabled feature modules, stored as jsonb (e.g. { "dispatch": true, ... }).</summary>
    public JsonDocument? Modules { get; set; }
}
