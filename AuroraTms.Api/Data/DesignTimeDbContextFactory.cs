using AuroraTms.Api.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuroraTms.Api.Data;

/// <summary>
/// Lets EF Core command-line tools (e.g. `dotnet ef migrations add ...`)
/// construct AppDbContext at design time. At runtime the real DbContext is built
/// by DI with the request-scoped tenant provider; this factory only exists so the
/// tooling can build the model (it never connects for migration scaffolding).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=aurora;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options, new TenantProvider());
    }
}
