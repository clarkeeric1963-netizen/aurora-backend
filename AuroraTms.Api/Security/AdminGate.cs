using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace AuroraTms.Api.Security;

public class AdminGateMiddleware
{
    private readonly RequestDelegate _next;
    public AdminGateMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value ?? "";

        bool isAdminArea = path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);
        bool isLoginRoute = path.StartsWith("/admin/login", StringComparison.OrdinalIgnoreCase)
                         || path.StartsWith("/admin/logout", StringComparison.OrdinalIgnoreCase);
        bool isPlatformApi = path.StartsWith("/api/customers", StringComparison.OrdinalIgnoreCase)
                          || path.StartsWith("/api/users", StringComparison.OrdinalIgnoreCase);

        bool needsGate = (isAdminArea && !isLoginRoute) || isPlatformApi;
        if (!needsGate || IsAuthed(ctx))
        {
            await _next(ctx);
            return;
        }

        if (isPlatformApi)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { error = "Admin login required." });
            return;
        }

        ctx.Response.Redirect("/admin/login.html");
    }

    public static string? ExpectedToken()
    {
        var pw = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if (string.IsNullOrEmpty(pw)) return null;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(pw));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes("aurora-admin-session-v1")));
    }

    private static bool IsAuthed(HttpContext ctx)
    {
        var expected = ExpectedToken();
        if (expected is null) return false;
        var cookie = ctx.Request.Cookies["admin_auth"];
        if (string.IsNullOrEmpty(cookie)) return false;
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(cookie), Encoding.UTF8.GetBytes(expected));
    }
}

[ApiController]
public class AdminAuthController : ControllerBase
{
    public record LoginRequest(string? Password);

    [HttpPost("/admin/login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var pw = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if (string.IsNullOrEmpty(pw))
            return StatusCode(503, new { error = "Admin login isn't configured yet. Set the ADMIN_PASSWORD variable." });

        var supplied = req?.Password ?? "";
        bool ok = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(supplied), Encoding.UTF8.GetBytes(pw));
        if (!ok)
            return Unauthorized(new { error = "Incorrect password." });

        Response.Cookies.Append("admin_auth", AdminGateMiddleware.ExpectedToken()!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromHours(8)
        });
        return Ok(new { ok = true });
    }

    [HttpPost("/admin/logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("admin_auth", new CookieOptions { Path = "/" });
        return Ok(new { ok = true });
    }
}
