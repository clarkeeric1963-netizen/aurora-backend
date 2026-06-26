using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using AuroraTms.Api.Tenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    public UsersController(AppDbContext db, ITenantProvider tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<IEnumerable<AppUser>> List() => await _db.Users.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> Get(string id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        return u is null ? NotFound() : u;
    }

    [HttpPost]
    public async Task<ActionResult<AppUser>> Create(AppUser u)
    {
        if (string.IsNullOrWhiteSpace(u.Id)) u.Id = $"USR-{Guid.NewGuid().ToString()[..8]}";
        u.TenantId = _tenant.TenantId!;
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = u.Id }, u);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, AppUser input)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
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
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return NotFound();
        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
