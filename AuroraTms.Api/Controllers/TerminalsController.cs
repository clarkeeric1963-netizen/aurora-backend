using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/terminals")]
public class TerminalsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TerminalsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Terminal>> List() => await _db.Terminals.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Terminal>> Get(string id)
    {
        var t = await _db.Terminals.FindAsync(id);
        return t is null ? NotFound() : t;
    }

    [HttpPost]
    public async Task<ActionResult<Terminal>> Create(Terminal t)
    {
        if (string.IsNullOrWhiteSpace(t.Id)) t.Id = $"TRM-{Guid.NewGuid().ToString()[..8]}";
        _db.Terminals.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Terminal input)
    {
        if (!await _db.Terminals.AnyAsync(x => x.Id == id)) return NotFound();
        input.Id = id;
        _db.Entry(input).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var t = await _db.Terminals.FindAsync(id);
        if (t is null) return NotFound();
        _db.Terminals.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
