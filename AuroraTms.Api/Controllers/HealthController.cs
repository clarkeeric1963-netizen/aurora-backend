using Microsoft.AspNetCore.Mvc;

namespace AuroraTms.Api.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", service = "AuroraTms.Api", time = DateTime.UtcNow });

    [HttpGet("")]
    public IActionResult Root() => Ok(new
    {
        service = "Aurora TMS API",
        version = "1.0",
        endpoints = new[] { "/health", "/api/customers", "/api/terminals", "/api/accounts", "/api/drivers", "/api/orders", "/api/users", "/swagger" }
    });
}
