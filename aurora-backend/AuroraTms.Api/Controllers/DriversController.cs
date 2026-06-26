using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/drivers")]
public class DriversController : ControllerBase
{
    private readonly AppDbContext _db;
    public DriversController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Driver>> List([FromQuery] string? terminal)
    {
        var q = _db.Drivers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(terminal))
            q = q.Where(d => d.HomeTerminal == terminal || d.TerminalCode == terminal);
        return await q.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Driver>> Get(string id)
    {
        var d = await _db.Drivers.FindAsync(id);
        return d is null ? NotFound() : d;
    }

    [HttpPost]
    public async Task<ActionResult<Driver>> Create(Driver d)
    {
        if (string.IsNullOrWhiteSpace(d.Id)) d.Id = $"DRV-{Guid.NewGuid().ToString()[..8]}";
        _db.Drivers.Add(d);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = d.Id }, d);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Driver input)
    {
        if (!await _db.Drivers.AnyAsync(x => x.Id == id)) return NotFound();
        input.Id = id;
        _db.Entry(input).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var d = await _db.Drivers.FindAsync(id);
        if (d is null) return NotFound();
        _db.Drivers.Remove(d);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
