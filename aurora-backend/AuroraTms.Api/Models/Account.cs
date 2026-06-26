using System.Text.Json;

namespace AuroraTms.Api.Models;

/// <summary>
/// A business account: shipper, consignee, or bill-to (maps to `accountsDataInit`).
/// </summary>
public class Account
{
    public string Id { get; set; } = default!;          // e.g. "ACCT-1001"
    public string Name { get; set; } = default!;
    public string? Type { get; set; }                   // Shipper | Consignee | Bill-To
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public string? MainPhone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Contact { get; set; }
    public string? HoursOpen { get; set; }
    public string? HoursClose { get; set; }
    public bool AppointmentReq { get; set; }
    public bool LiftGateReq { get; set; }
    public string? DefaultBillTo { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public bool RequiresSignedDocs { get; set; }
    public string? SignedDocType { get; set; }
    public int SignedDocCopies { get; set; }
    public string? SignedDocReturn { get; set; }
    public string? Notes { get; set; }

    /// <summary>EDI transaction sets enabled (e.g. ["204","990","214","210"]) — jsonb.</summary>
    public JsonDocument? EdiSets { get; set; }

    /// <summary>Invoicing/billing preferences — jsonb.</summary>
    public JsonDocument? Invoicing { get; set; }

    /// <summary>Notification preferences / webhook config — jsonb.</summary>
    public JsonDocument? NotifPrefs { get; set; }
}
