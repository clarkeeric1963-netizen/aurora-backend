using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<AppUser>> List() => await _db.Users.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> Get(string id)
    {
        var u = await _db.Users.FindAsync(id);
        return u is null ? NotFound() : u;
    }

    [HttpPost]
    public async Task<ActionResult<AppUser>> Create(AppUser u)
    {
        if (string.IsNullOrWhiteSpace(u.Id)) u.Id = $"USR-{Guid.NewGuid().ToString()[..8]}";
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = u.Id }, u);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, AppUser input)
    {
        if (!await _db.Users.AnyAsync(x => x.Id == id)) return NotFound();
        input.Id = id;
        _db.Entry(input).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u is null) return NotFound();
        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
