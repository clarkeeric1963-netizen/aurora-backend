using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Customer>> List() => await _db.Customers.AsNoTracking().ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> Get(string id)
    {
        var c = await _db.Customers.FindAsync(id);
        return c is null ? NotFound() : c;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer c)
    {
        if (string.IsNullOrWhiteSpace(c.Id)) c.Id = $"CUS-{Guid.NewGuid().ToString()[..8]}";
        _db.Customers.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Customer input)
    {
        if (!await _db.Customers.AnyAsync(x => x.Id == id)) return NotFound();
        input.Id = id;
        _db.Entry(input).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        _db.Customers.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
