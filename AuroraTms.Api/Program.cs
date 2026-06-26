using AuroraTms.Api.Data;
using AuroraTms.Api.Tenancy;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

string connString = BuildConnectionString(builder.Configuration);

builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connString));

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<AuroraTms.Api.Security.AdminGateMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.UseMiddleware<TenantResolutionMiddleware>();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

static string BuildConnectionString(IConfiguration config)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var b = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Prefer,
            TrustServerCertificate = true,
            Pooling = true
        };
        return b.ConnectionString;
    }

    var local = config.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(local))
        throw new InvalidOperationException(
            "No database configured. Set DATABASE_URL or ConnectionStrings:Default.");
    return local;
}
