using System.Text.Json;
using AuroraTms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuroraTms.Api.Data;

/// <summary>
/// Seeds a representative slice of the app's mock data across TWO tenants so you
/// can prove isolation: query as one tenant and you only see that tenant's rows.
/// Idempotent: each block runs only when its table is empty. Uses
/// IgnoreQueryFilters so the tenant filters don't hide existing rows during seeding.
/// </summary>
public static class DbSeeder
{
    private static JsonDocument J(string json) => JsonDocument.Parse(json);

    // Tenant ids (these live in the Customers registry).
    private const string T1 = "CUS-0001"; // Southeastern Freight Lines  -> subdomain "sefl"
    private const string T2 = "CUS-0002"; // Gulf States Logistics       -> subdomain "gulfstates"

    public static void Seed(AppDbContext db)
    {
        if (!db.Customers.Any())
        {
            db.Customers.AddRange(
                new Customer { Id = T1, Subdomain = "sefl", Company = "Southeastern Freight Lines", Contact = "Robert Caldwell", Email = "rcaldwell@sefl.com", Phone = "(404) 555-1200", Tier = "enterprise", Status = "Active", Terminals = 28, Users = 340, Drivers = 1200, OrdersMonth = 85000, Domain = "sefl.com", Industry = "LTL Carrier", Region = "Southeast US", Mrr = 8500m, Notes = "Flagship enterprise client. Custom SLA.", SetupComplete = true, CreatedAt = new DateOnly(2023, 1, 15), Modules = J("{\"dispatch\":true,\"routing\":true,\"dock\":true,\"edi\":true,\"api\":true,\"mobile\":true,\"hos\":true,\"analytics\":true,\"branding\":true}") },
                new Customer { Id = T2, Subdomain = "gulfstates", Company = "Gulf States Logistics", Contact = "Patricia Nguyen", Email = "pnguyen@gulfstates.com", Phone = "(713) 555-8800", Tier = "pro-plus", Status = "Active", Terminals = 6, Users = 42, Drivers = 155, OrdersMonth = 18500, Domain = "gulfstates.com", Industry = "3PL", Region = "Gulf Coast", Mrr = 1499m, Notes = "Upgraded from Pro Q3 2024.", SetupComplete = true, CreatedAt = new DateOnly(2023, 6, 22), Modules = J("{\"dispatch\":true,\"routing\":true,\"dock\":true,\"edi\":true,\"api\":true,\"mobile\":true,\"hos\":true,\"analytics\":true,\"branding\":false}") },
                new Customer { Id = "CUS-0003", Subdomain = "apex", Company = "Apex Regional Transport", Contact = "David Kim", Email = "dkim@apexregional.com", Phone = "(919) 555-3300", Tier = "pro", Status = "Active", Terminals = 2, Users = 12, Drivers = 38, OrdersMonth = 3200, Domain = "apexregional.com", Industry = "Regional Carrier", Region = "Carolinas", Mrr = 799m, SetupComplete = true, CreatedAt = new DateOnly(2024, 2, 10), Modules = J("{\"dispatch\":true,\"routing\":true,\"dock\":false,\"edi\":true,\"api\":false,\"mobile\":true,\"hos\":false,\"analytics\":false,\"branding\":false}") }
            );
        }

        if (!db.Terminals.IgnoreQueryFilters().Any())
        {
            db.Terminals.AddRange(
                new Terminal { Id = "TRM-001", TenantId = T1, Name = "Orlando Terminal", Code = "ORL", Lat = 28.5721, Lng = -81.3617, Address = "2840 N Orange Blossom Trl", City = "Orlando", State = "FL", Zip = "32804", Country = "US", Phone = "(407) 555-0100", Email = "ops@orlando.aurora-tms.com", Status = "Active", Docks = 18, YardSpots = 40, Timezone = "EST", Manager = "David Chen", OperatingHours = "5:00 AM \u2013 10:00 PM", Type = "Hub", IsAirport = true, IsWhse = true, Sqft = "42,000", Region = "Southeast", DotNumber = "DOT-1027849", Scac = "LYKS", Notes = "Primary hub for FL corridor.", DockConfig = J("{\"surface\":\"Concrete\",\"levelerType\":\"Hydraulic\",\"doorsSideA\":12,\"doorsSideB\":6}"), YardConfig = J("{\"totalSpots\":40,\"zones\":[{\"id\":\"A\",\"name\":\"Lot A\",\"spots\":12,\"type\":\"Staging\"}]}") },
                new Terminal { Id = "TRM-101", TenantId = T2, Name = "Houston Terminal", Code = "HOU", Lat = 29.7604, Lng = -95.3698, Address = "1200 Gulf Fwy", City = "Houston", State = "TX", Zip = "77003", Country = "US", Phone = "(713) 555-0100", Email = "ops@houston.aurora-tms.com", Status = "Active", Docks = 10, YardSpots = 24, Timezone = "CST", Manager = "Elena Ruiz", OperatingHours = "6:00 AM \u2013 9:00 PM", Type = "Hub", Region = "Gulf Coast", Scac = "GULF", Notes = "Gulf States primary hub." }
            );
        }

        if (!db.Accounts.IgnoreQueryFilters().Any())
        {
            db.Accounts.Add(new Account
            {
                Id = "ACCT-1001", TenantId = T1, Name = "Greenfield Logistics LLC", Type = "Shipper",
                Address = "2840 N Orange Blossom Trl", Address2 = "Suite 200", City = "Orlando", State = "FL", Zip = "32804", Country = "US",
                MainPhone = "(407) 555-0122", Email = "dispatch@greenfield.com", Contact = "Maria Gutierrez",
                HoursOpen = "06:00 AM", HoursClose = "06:00 PM", DefaultBillTo = "ACCT-1001",
                Lat = 28.5697, Lng = -81.3877, RequiresSignedDocs = true, SignedDocType = "Customer BOL", SignedDocCopies = 2, SignedDocReturn = "Scan OK",
                Notes = "Loading dock accessible from rear lot.",
                EdiSets = J("[\"204\",\"990\",\"214\",\"210\"]"),
                Invoicing = J("{\"billingMethod\":\"FB && Images\",\"paymentTerms\":\"Net 30\",\"dueDays\":15}"),
                NotifPrefs = J("{\"sms\":true,\"email\":true,\"triggers\":[\"pickup\",\"in_transit\",\"delivered\"]}")
            });
        }

        if (!db.Drivers.IgnoreQueryFilters().Any())
        {
            db.Drivers.AddRange(
                new Driver { Id = "DRV-2001", TenantId = T1, Name = "Marcus Johnson", FirstName = "Marcus", LastName = "Johnson", Pin = "2001", Status = "On Duty", Photo = "https://randomuser.me/api/portraits/men/32.jpg", PhotoColor = "#3b82f6", Cell = "(407) 555-1002", Email = "m.johnson@sefl.com", CurrentLocation = "I-75 near Valdosta, GA", DefaultEquip = "Tractor #1002 / Dry Van #2002", AssignedEquip = "Tractor #1002 / Dry Van #2002", CurrentOrder = "ORL-10018", CdlClass = "A", CdlNumber = "J429-118-72-401", CdlState = "FL", CdlExpiry = "08/15/2027", MedCertExpiry = "12/05/2026", HomeTerminal = "ORL", TerminalCode = "ORL", Capacity = 44000, AvailableCapacity = 5800, HireDate = "03/15/2022", Dob = "06/22/1988", Address = "1420 Magnolia Ave", City = "Orlando", State = "FL", Zip = "32803", EmergencyContact = "Lisa Johnson", EmergencyPhone = "(407) 555-1003", PayType = "Per Mile", PayRate = 0.58m, HosStatus = "Driving", HosClock = "6:22 remaining", HosViolations = 0, Mvr = "Clean", TwicCard = true, Hazmat = false, AccidentCount = 0, Notes = "Senior driver.", Endorsements = J("[\"T\",\"N\"]"), Restrictions = J("[]") },
                new Driver { Id = "DRV-3001", TenantId = T2, Name = "Carlos Mendez", FirstName = "Carlos", LastName = "Mendez", Pin = "3001", Status = "Available", PhotoColor = "#10b981", Cell = "(713) 555-3002", Email = "c.mendez@gulfstates.com", CurrentLocation = "Houston Terminal", DefaultEquip = "Tractor #3002 / Reefer #4002", HomeTerminal = "HOU", TerminalCode = "HOU", Capacity = 42000, AvailableCapacity = 42000, CdlClass = "A", CdlState = "TX", PayType = "Per Mile", PayRate = 0.55m, HosStatus = "Off Duty", AccidentCount = 0, Endorsements = J("[\"N\"]"), Restrictions = J("[]") }
            );
        }

        if (!db.Users.IgnoreQueryFilters().Any())
        {
            db.Users.AddRange(
                new AppUser { Id = "ADM-001", TenantId = T1, Name = "James Donovan", Email = "j.donovan@sefl.com", Role = "Company Admin", Dept = "Executive", IsAdmin = true, Initials = "JD", Phone = "(407) 555-0001", Status = "Active", InviteStatus = "Accepted", Permissions = J("[\"Full Access\",\"Admin\",\"Reports\",\"Billing\",\"Setup\"]"), Modules = J("[\"ops\",\"dispatch\",\"orders\",\"drivers\",\"terminals\",\"billing\",\"reports\",\"setup\"]"), TerminalAccess = J("[\"ALL\"]") },
                new AppUser { Id = "ADM-101", TenantId = T2, Name = "Patricia Nguyen", Email = "p.nguyen@gulfstates.com", Role = "Company Admin", Dept = "Executive", IsAdmin = true, Initials = "PN", Phone = "(713) 555-0001", Status = "Active", InviteStatus = "Accepted", Permissions = J("[\"Full Access\",\"Admin\"]"), Modules = J("[\"ops\",\"dispatch\",\"orders\",\"drivers\"]"), TerminalAccess = J("[\"ALL\"]") }
            );
        }

        if (!db.Orders.IgnoreQueryFilters().Any())
        {
            db.Orders.AddRange(
                new Order
                {
                    Id = "ORL-10018", TenantId = T1, Customer = "Greenfield Logistics", Origin = "Orlando, FL", Dest = "Atlanta, GA",
                    Driver = "Marcus Johnson", Equipment = "Dry Van #1042", Status = "In Transit", Priority = "High",
                    OrderDate = "04/03/2026", Pickup = "04/05/2026", Delivery = "04/06/2026", Weight = "38,200", Rate = "$2,450.00",
                    Bol = "BOL-88291034", ShipType = "Truckload", Billing = "Prepaid", RefNum = "GF-20260227-A",
                    Shipper = J("{\"name\":\"Greenfield Logistics LLC\",\"city\":\"Orlando\",\"state\":\"FL\",\"zip\":\"32804\"}"),
                    Consignee = J("{\"name\":\"Southeast Distribution Hub\",\"city\":\"Atlanta\",\"state\":\"GA\",\"zip\":\"30309\"}"),
                    BillTo = J("{\"name\":\"Greenfield Logistics LLC\",\"city\":\"Orlando\",\"state\":\"FL\",\"zip\":\"32804\"}"),
                    LineItems = new List<OrderLineItem>
                    {
                        new() { Cls = "65", Sku = "BWS-2400", Desc = "Sparkling Water 24pk", Wt = "1080", Cube = "33.0", Pieces = 60, Unit = "pcs" },
                        new() { Cls = "85", Sku = "POO-1200", Desc = "Premium Olive Oil 1L x12", Wt = "576", Cube = "19.2", Pieces = 48, Unit = "pcs" }
                    }
                },
                new Order
                {
                    Id = "HOU-20001", TenantId = T2, Customer = "Bayou Foods", Origin = "Houston, TX", Dest = "New Orleans, LA",
                    Driver = "Carlos Mendez", Equipment = "Reefer #4002", Status = "Planning", Priority = "Medium",
                    OrderDate = "04/04/2026", Pickup = "04/07/2026", Delivery = "04/08/2026", Weight = "41,000", Rate = "$1,980.00",
                    Bol = "BOL-77110022", ShipType = "Truckload", Billing = "Collect", RefNum = "BF-44210",
                    Shipper = J("{\"name\":\"Bayou Foods Inc\",\"city\":\"Houston\",\"state\":\"TX\",\"zip\":\"77003\"}"),
                    Consignee = J("{\"name\":\"Crescent City Grocers\",\"city\":\"New Orleans\",\"state\":\"LA\",\"zip\":\"70112\"}"),
                    LineItems = new List<OrderLineItem>
                    {
                        new() { Cls = "70", Sku = "FRZ-0500", Desc = "Frozen Shrimp 5lb", Wt = "2000", Cube = "40.0", Pieces = 400, Unit = "cs" }
                    }
                }
            );
        }

        db.SaveChanges();
    }
}
