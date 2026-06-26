using AuroraTms.Api.Models;
using AuroraTms.Api.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenant) : base(options)
        => _tenant = tenant;

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLineItem> OrderLineItems => Set<OrderLineItem>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<YardMove> YardMoves => Set<YardMove>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ---- Customer (the tenant registry -- NOT tenant-filtered) ----
        b.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Subdomain).IsUnique();
            e.Property(x => x.Mrr).HasColumnType("numeric(12,2)");
            e.Property(x => x.Discount).HasColumnType("numeric(6,2)");
            e.Property(x => x.Modules).HasColumnType("jsonb");
        });

        // ---- Terminal ----
        b.Entity<Terminal>(e =>
        {
            e.ToTable("terminals");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.Code);
            e.Property(x => x.DockConfig).HasColumnType("jsonb");
            e.Property(x => x.YardConfig).HasColumnType("jsonb");
            // Auto-scope every query to the current tenant.
            e.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        // ---- Account ----
        b.Entity<Account>(e =>
        {
            e.ToTable("accounts");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            e.Property(x => x.EdiSets).HasColumnType("jsonb");
            e.Property(x => x.Invoicing).HasColumnType("jsonb");
            e.Property(x => x.NotifPrefs).HasColumnType("jsonb");
            e.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        // ---- Driver ----
        b.Entity<Driver>(e =>
        {
            e.ToTable("drivers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.HomeTerminal);
            e.Property(x => x.PayRate).HasColumnType("numeric(8,4)");
            e.Property(x => x.Endorsements).HasColumnType("jsonb");
            e.Property(x => x.Restrictions).HasColumnType("jsonb");
            e.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        // ---- Order + line items ----
        b.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.Customer);
            e.Property(x => x.Shipper).HasColumnType("jsonb");
            e.Property(x => x.Consignee).HasColumnType("jsonb");
            e.Property(x => x.BillTo).HasColumnType("jsonb");
            e.HasMany(x => x.LineItems)
                .WithOne(li => li.Order!)
                .HasForeignKey(li => li.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        b.Entity<OrderLineItem>(e =>
        {
            e.ToTable("order_line_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            // Scope direct line-item queries through the parent order's tenant.
            e.HasQueryFilter(li => li.Order!.TenantId == _tenant.TenantId);
        });

        // ---- User ----
        b.Entity<AppUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            // Email is unique within a tenant (the same email may exist in two tenants).
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            e.Property(x => x.Permissions).HasColumnType("jsonb");
            e.Property(x => x.Modules).HasColumnType("jsonb");
            e.Property(x => x.TerminalAccess).HasColumnType("jsonb");
            e.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        // ---- Yard move ----
        b.Entity<YardMove>(e =>
        {
            e.ToTable("yard_moves");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.TerminalId);
            e.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });
    }
}
