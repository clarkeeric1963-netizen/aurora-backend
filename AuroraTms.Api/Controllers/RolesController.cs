using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

/// <summary>
/// The GLOBAL role catalog (owned by Aurora, shared by all tenants). Unlike
/// every other controller, this is NOT tenant-scoped — roles are global
/// definitions. Writes are protected by the admin gate (AdminGateMiddleware
/// gates /api/roles). A user's assigned role name references one of these.
/// </summary>
[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;
    public RolesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IEnumerable<RoleDef>> List()
        => await _db.Roles.AsNoTracking().OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDef>> Get(string id)
    {
        var r = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id);
        return r is null ? NotFound() : r;
    }

    [HttpPost]
    public async Task<ActionResult<RoleDef>> Create(RoleDef r)
    {
        if (string.IsNullOrWhiteSpace(r.Id))
            r.Id = "ROLE-" + (r.Name ?? Guid.NewGuid().ToString()[..8]).Trim().ToLowerInvariant().Replace(' ', '-');
        if (await _db.Roles.AnyAsync(x => x.Id == r.Id))
            return Conflict(new { error = $"A role with id '{r.Id}' already exists." });
        _db.Roles.Add(r);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = r.Id }, r);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, RoleDef input)
    {
        var existing = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return NotFound();
        existing.Name = input.Name;
        existing.Description = input.Description;
        existing.Modules = input.Modules;
        existing.SortOrder = input.SortOrder;
        existing.IsSystem = input.IsSystem;
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var r = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();
        if (r.IsSystem) return BadRequest(new { error = "System roles can't be deleted." });
        _db.Roles.Remove(r);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
