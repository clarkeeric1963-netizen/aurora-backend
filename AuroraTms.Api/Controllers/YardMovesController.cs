using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using AuroraTms.Api.Tenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/yardmoves")]
public class YardMovesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    public YardMovesController(AppDbContext db, ITenantProvider tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<IEnumerable<YardMove>> List([FromQuery] string? status, [FromQuery] string? terminal)
    {
        var q = _db.YardMoves.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(m => m.Status == status);
        if (!string.IsNullOrWhiteSpace(terminal)) q = q.Where(m => m.TerminalId == terminal);
        return await q.OrderBy(m => m.Status).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<YardMove>> Get(string id)
    {
        var m = await _db.YardMoves.FirstOrDefaultAsync(x => x.Id == id);
        return m is null ? NotFound() : m;
    }

    [HttpPost]
    public async Task<ActionResult<YardMove>> Create(YardMove m)
    {
        if (string.IsNullOrWhiteSpace(m.Id)) m.Id = $"YM-{Guid.NewGuid().ToString()[..8]}";
        m.TenantId = _tenant.TenantId!;
        if (string.IsNullOrWhiteSpace(m.Status)) m.Status = "Assigned";
        m.CreatedAt ??= DateTime.UtcNow.ToString("o");
        _db.YardMoves.Add(m);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = m.Id }, m);
    }

    public record StatusUpdate(string Status);

    [HttpPut("{id}/status")]
    public async Task<ActionResult<YardMove>> SetStatus(string id, [FromBody] StatusUpdate body)
    {
        var m = await _db.YardMoves.FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return NotFound();
        var s = (body?.Status ?? "").Trim();
        if (s != "Assigned" && s != "In Progress" && s != "Done")
            return BadRequest(new { error = "Status must be Assigned, In Progress, or Done." });

        m.Status = s;
        if (s == "In Progress" && string.IsNullOrEmpty(m.StartedAt)) m.StartedAt = DateTime.UtcNow.ToString("o");
        if (s == "Done") m.CompletedAt = DateTime.UtcNow.ToString("o");
        await _db.SaveChangesAsync();
        return m;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, YardMove input)
    {
        var existing = await _db.YardMoves.FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return NotFound();
        input.Id = id;
        input.TenantId = existing.TenantId;
        _db.Entry(existing).CurrentValues.SetValues(input);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var m = await _db.YardMoves.FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return NotFound();
        _db.YardMoves.Remove(m);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
