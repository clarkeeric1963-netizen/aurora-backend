using Microsoft.AspNetCore.Mvc;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", service = "AuroraTms.Api", time = DateTime.UtcNow });

    // Moved off "/" so the bundled app (wwwroot/index.html) serves at the root.
    [HttpGet("/info")]
    public IActionResult Info() => Ok(new
    {
        service = "Aurora TMS API",
        version = "1.0",
        endpoints = new[] { "/health", "/api/customers", "/api/terminals", "/api/accounts", "/api/drivers", "/api/orders", "/api/users", "/swagger" }
    });
}
