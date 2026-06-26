using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AccountsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Account>> List() => await _db.Accounts.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> Get(string id)
    {
        var a = await _db.Accounts.FindAsync(id);
        return a is null ? NotFound() : a;
    }

    [HttpPost]
    public async Task<ActionResult<Account>> Create(Account a)
    {
        if (string.IsNullOrWhiteSpace(a.Id)) a.Id = $"ACCT-{Guid.NewGuid().ToString()[..8]}";
        _db.Accounts.Add(a);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = a.Id }, a);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Account input)
    {
        if (!await _db.Accounts.AnyAsync(x => x.Id == id)) return NotFound();
        input.Id = id;
        _db.Entry(input).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var a = await _db.Accounts.FindAsync(id);
        if (a is null) return NotFound();
        _db.Accounts.Remove(a);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
