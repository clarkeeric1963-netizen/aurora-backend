using AuroraTms.Api.Data;
using AuroraTms.Api.Tenancy;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. Bind to the port Railway provides (Railway sets the PORT env var).
// ---------------------------------------------------------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------------------------------------------------------------------------
// 2. Build the Npgsql connection string.
//    Railway's Postgres plugin injects DATABASE_URL in the URI form:
//      postgresql://user:pass@host:port/dbname
//    Npgsql needs a key/value string, so we translate it. Locally you can set
//    ConnectionStrings__Default instead (see appsettings.Development.json).
// ---------------------------------------------------------------------------
string connString = BuildConnectionString(builder.Configuration);

// Per-request tenant context, read by AppDbContext's global query filters.
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connString));

// ---------------------------------------------------------------------------
// 3. Controllers, JSON, CORS, Swagger.
// ---------------------------------------------------------------------------
builder.Services.AddControllers().AddJsonOptions(o =>
{
    // Emit camelCase to match the React frontend's field names.
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    // Break parent/child reference loops (order -> line items -> order) so JSON doesn't throw.
    o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------------------------------------------------------------------------
// 4. Create the schema and seed on first boot.
//    For real schema versioning, swap EnsureCreated() for Migrate() after you
//    generate migrations (see README). EnsureCreated is fine to get live fast.
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // EnsureCreated() doesn't add new tables to an already-existing database.
    // Create the yard_moves table if it's missing (so we don't lose existing data),
    // then seed a few demo moves. Idempotent.
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS yard_moves (
            ""Id"" text NOT NULL,
            ""TenantId"" text NOT NULL,
            ""TerminalId"" text NULL,
            ""Type"" text NULL,
            ""Trailer"" text NULL,
            ""FromLoc"" text NULL,
            ""ToLoc"" text NULL,
            ""Priority"" text NULL,
            ""Urgent"" boolean NOT NULL DEFAULT FALSE,
            ""Reason"" text NULL,
            ""Manifest"" text NULL,
            ""Jockey"" text NULL,
            ""Status"" text NOT NULL DEFAULT 'Assigned',
            ""CreatedAt"" text NULL,
            ""StartedAt"" text NULL,
            ""CompletedAt"" text NULL,
            CONSTRAINT ""PK_yard_moves"" PRIMARY KEY (""Id"")
        );");
    db.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_yard_moves_TenantId"" ON yard_moves (""TenantId"");");
    db.Database.ExecuteSqlRaw(@"
        INSERT INTO yard_moves (""Id"",""TenantId"",""TerminalId"",""Type"",""Trailer"",""FromLoc"",""ToLoc"",""Priority"",""Urgent"",""Reason"",""Manifest"",""Jockey"",""Status"",""CreatedAt"")
        VALUES
        ('YM-0001','CUS-0001','TRM-001','Spot','TRL-9903','Lot B-4','Door D-07','High',TRUE,'ORL-012 arrived - strip 3 orders','ORL-012','Marcus Johnson','Assigned', now()::text),
        ('YM-0002','CUS-0001','TRM-001','Pull','TRL-7001','Door D-03','Lot C-1','Normal',FALSE,'Fluid sealed at cut time - stage for dispatch','ORL-010','Marcus Johnson','Assigned', now()::text),
        ('YM-0003','CUS-0001','TRM-001','Spot','TRL-EMPTY-04','Lot D-2','Door D-09','Normal',FALSE,'New MIA manifest - empty trailer needed','NEW','Marcus Johnson','Assigned', now()::text)
        ON CONFLICT (""Id"") DO NOTHING;");

    DbSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI();

// Gate the super-admin surface (/admin UI + platform registry) before anything
// serves it. Stopgap until full auth is built.
app.UseMiddleware<AuroraTms.Api.Security.AdminGateMiddleware>();

// Serve the bundled frontend (wwwroot/index.html + assets). Static files are
// served before tenant resolution so the UI always loads.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

// Resolve the tenant (from subdomain or X-Tenant header) on every request,
// before any controller runs, so all DB queries are tenant-scoped.
app.UseMiddleware<TenantResolutionMiddleware>();

app.MapControllers();

// Anything not matched by an API route or a static file returns the app shell,
// so the single-page UI loads at the domain root (and any client-side route).
app.MapFallbackToFile("index.html");

app.Run();


// ---------------------------------------------------------------------------
// Helper: turn DATABASE_URL (or ConnectionStrings:Default) into an Npgsql string.
// ---------------------------------------------------------------------------
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
            SslMode = SslMode.Prefer,           // Railway public networking uses TLS
            TrustServerCertificate = true,
            Pooling = true
        };
        return b.ConnectionString;
    }

    // Fallback for local dev.
    var local = config.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(local))
        throw new InvalidOperationException(
            "No database configured. Set DATABASE_URL or ConnectionStrings:Default.");
    return local;
}
