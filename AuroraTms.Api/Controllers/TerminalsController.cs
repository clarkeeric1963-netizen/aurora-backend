using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using AuroraTms.Api.Tenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/terminals")]
public class TerminalsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    public TerminalsController(AppDbContext db, ITenantProvider tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<IEnumerable<Terminal>> List() => await _db.Terminals.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Terminal>> Get(string id)
    {
        var t = await _db.Terminals.FirstOrDefaultAsync(x => x.Id == id);
        return t is null ? NotFound() : t;
    }

    [HttpPost]
    public async Task<ActionResult<Terminal>> Create(Terminal t)
    {
        if (string.IsNullOrWhiteSpace(t.Id)) t.Id = $"TRM-{Guid.NewGuid().ToString()[..8]}";
        t.TenantId = _tenant.TenantId!;
        _db.Terminals.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Terminal input)
    {
        var existing = await _db.Terminals.FirstOrDefaultAsync(x => x.Id == id);
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
        var t = await _db.Terminals.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        _db.Terminals.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
