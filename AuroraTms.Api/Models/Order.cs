using System.Text.Json;

namespace AuroraTms.Api.Models;

/// <summary>
/// A freight order / load (maps to the app's `ordersDataInit` records).
/// </summary>
public class Order
{
    public string Id { get; set; } = default!;          // e.g. "ORL-10018"
    public string? Customer { get; set; }
    public string? Origin { get; set; }
    public string? Dest { get; set; }
    public string? Driver { get; set; }
    public string? Equipment { get; set; }
    public string Status { get; set; } = "Planning";    // Planning | Dispatched | In Transit | Delivered | ...
    public string? Priority { get; set; }               // High | Medium | Low
    public string? OrderDate { get; set; }
    public string? Pickup { get; set; }
    public string? Delivery { get; set; }
    public string? Weight { get; set; }
    public string? Rate { get; set; }
    public string? Bol { get; set; }
    public string? ShipType { get; set; }               // Truckload | LTL | ...
    public string? Billing { get; set; }                // Prepaid | Collect | 3rd Party
    public string? RefNum { get; set; }

    /// <summary>Shipper address block — jsonb.</summary>
    public JsonDocument? Shipper { get; set; }

    /// <summary>Consignee address block — jsonb.</summary>
    public JsonDocument? Consignee { get; set; }

    /// <summary>Bill-to address block — jsonb.</summary>
    public JsonDocument? BillTo { get; set; }

    public List<OrderLineItem> LineItems { get; set; } = new();
}

/// <summary>
/// A single line item on an order (the app's `lineItems[]`).
/// </summary>
public class OrderLineItem
{
    public int Id { get; set; }                         // surrogate key
    public string OrderId { get; set; } = default!;
    public Order? Order { get; set; }

    public string? Cls { get; set; }                    // freight class, e.g. "65"
    public string? Sku { get; set; }
    public string? Desc { get; set; }
    public string? Wt { get; set; }
    public string? Cube { get; set; }
    public int Pieces { get; set; }
    public string? Unit { get; set; }
}
