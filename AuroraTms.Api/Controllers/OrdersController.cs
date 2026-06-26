using AuroraTms.Api.Data;
using AuroraTms.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Order>> List([FromQuery] string? status, [FromQuery] string? customer)
    {
        var q = _db.Orders.AsNoTracking().Include(o => o.LineItems).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);
        if (!string.IsNullOrWhiteSpace(customer)) q = q.Where(o => o.Customer == customer);
        return await q.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> Get(string id)
    {
        var o = await _db.Orders.Include(x => x.LineItems).FirstOrDefaultAsync(x => x.Id == id);
        return o is null ? NotFound() : o;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(Order o)
    {
        if (string.IsNullOrWhiteSpace(o.Id)) o.Id = $"ORD-{Guid.NewGuid().ToString()[..8]}";
        foreach (var li in o.LineItems) li.OrderId = o.Id;
        _db.Orders.Add(o);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = o.Id }, o);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Order input)
    {
        var existing = await _db.Orders.Include(x => x.LineItems).FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return NotFound();

        // scalar + jsonb fields
        _db.Entry(existing).CurrentValues.SetValues(input);
        existing.Id = id;

        // replace line items
        _db.OrderLineItems.RemoveRange(existing.LineItems);
        existing.LineItems = input.LineItems;
        foreach (var li in existing.LineItems) { li.Id = 0; li.OrderId = id; }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var o = await _db.Orders.FindAsync(id);
        if (o is null) return NotFound();
        _db.Orders.Remove(o);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
