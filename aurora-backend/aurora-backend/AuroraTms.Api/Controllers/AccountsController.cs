using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using AuroraTms.Api.Tenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    public AccountsController(AppDbContext db, ITenantProvider tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<IEnumerable<Account>> List() => await _db.Accounts.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> Get(string id)
    {
        var a = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        return a is null ? NotFound() : a;
    }

    [HttpPost]
    public async Task<ActionResult<Account>> Create(Account a)
    {
        if (string.IsNullOrWhiteSpace(a.Id)) a.Id = $"ACCT-{Guid.NewGuid().ToString()[..8]}";
        a.TenantId = _tenant.TenantId!;
        _db.Accounts.Add(a);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = a.Id }, a);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Account input)
    {
        var existing = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == id);
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
        var a = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound();
        _db.Accounts.Remove(a);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
