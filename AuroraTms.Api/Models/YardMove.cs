namespace AuroraTms.Api.Models;

/// <summary>
/// A yard task: reposition a trailer within a terminal. Spot (bring to a door),
/// Pull (take from a door to a lot), or a general Move. Tenant- and terminal-scoped.
/// </summary>
public class YardMove
{
    public string Id { get; set; } = default!;          // e.g. "YM-0001"
    public string TenantId { get; set; } = default!;
    public string? TerminalId { get; set; }

    public string? Type { get; set; }                   // Spot | Pull | Move
    public string? Trailer { get; set; }
    public string? FromLoc { get; set; }
    public string? ToLoc { get; set; }
    public string? Priority { get; set; }               // High | Normal | Low
    public bool Urgent { get; set; }
    public string? Reason { get; set; }
    public string? Manifest { get; set; }               // related order / manifest ref
    public string? Jockey { get; set; }                 // assigned jockey

    public string Status { get; set; } = "Assigned";    // Assigned | In Progress | Done

    public string? CreatedAt { get; set; }
    public string? StartedAt { get; set; }
    public string? CompletedAt { get; set; }
}
