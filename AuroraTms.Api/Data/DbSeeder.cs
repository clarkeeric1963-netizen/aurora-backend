using System.Text.Json;
using AuroraTms.Api.Models;

namespace AuroraTms.Api.Data;

/// <summary>
/// Seeds a representative slice of the app's mock data so the live DB isn't empty.
/// Runs only when the relevant table is empty (idempotent on first boot).
/// </summary>
public static class DbSeeder
{
    private static JsonDocument J(string json) => JsonDocument.Parse(json);

    public static void Seed(AppDbContext db)
    {
        if (!db.Customers.Any())
        {
            db.Customers.AddRange(
                new Customer { Id = "CUS-0001", Company = "Southeastern Freight Lines", Contact = "Robert Caldwell", Email = "rcaldwell@sefl.com", Phone = "(404) 555-1200", Tier = "enterprise", Status = "Active", Terminals = 28, Users = 340, Drivers = 1200, OrdersMonth = 85000, Domain = "sefl.com", Industry = "LTL Carrier", Region = "Southeast US", Mrr = 8500m, Notes = "Flagship enterprise client. Custom SLA.", SetupComplete = true, CreatedAt = new DateOnly(2023, 1, 15), Modules = J("{\"dispatch\":true,\"routing\":true,\"dock\":true,\"edi\":true,\"api\":true,\"mobile\":true,\"hos\":true,\"analytics\":true,\"branding\":true}") },
                new Customer { Id = "CUS-0002", Company = "Gulf States Logistics", Contact = "Patricia Nguyen", Email = "pnguyen@gulfstates.com", Phone = "(713) 555-8800", Tier = "pro-plus", Status = "Active", Terminals = 6, Users = 42, Drivers = 155, OrdersMonth = 18500, Domain = "gulfstates.com", Industry = "3PL", Region = "Gulf Coast", Mrr = 1499m, Notes = "Upgraded from Pro Q3 2024.", SetupComplete = true, CreatedAt = new DateOnly(2023, 6, 22), Modules = J("{\"dispatch\":true,\"routing\":true,\"dock\":true,\"edi\":true,\"api\":true,\"mobile\":true,\"hos\":true,\"analytics\":true,\"branding\":false}") },
                new Customer { Id = "CUS-0003", Company = "Apex Regional Transport", Contact = "David Kim", Email = "dkim@apexregional.com", Phone = "(919) 555-3300", Tier = "pro", Status = "Active", Terminals = 2, Users = 12, Drivers = 38, OrdersMonth = 3200, Domain = "apexregional.com", Industry = "Regional Carrier", Region = "Carolinas", Mrr = 799m, SetupComplete = true, CreatedAt = new DateOnly(2024, 2, 10), Modules = J("{\"dispatch\":true,\"routing\":true,\"dock\":false,\"edi\":true,\"api\":false,\"mobile\":true,\"hos\":false,\"analytics\":false,\"branding\":false}") }
            );
        }

        if (!db.Terminals.Any())
        {
            db.Terminals.Add(new Terminal
            {
                Id = "TRM-001", Name = "Orlando Terminal", Code = "ORL", Lat = 28.5721, Lng = -81.3617,
                Address = "2840 N Orange Blossom Trl", City = "Orlando", State = "FL", Zip = "32804", Country = "US",
                Phone = "(407) 555-0100", Fax = "(407) 555-0199", Email = "ops@orlando.aurora-tms.com", Status = "Active",
                Docks = 18, YardSpots = 40, Timezone = "EST", Manager = "David Chen", OperatingHours = "5:00 AM \u2013 10:00 PM",
                Type = "Hub", IsAirport = true, IsWhse = true, Sqft = "42,000", Region = "Southeast", DotNumber = "DOT-1027849", Scac = "LYKS",
                Notes = "Primary hub for FL corridor.",
                DockConfig = J("{\"numMethod\":\"oddeven\",\"surface\":\"Concrete\",\"levelerType\":\"Hydraulic\",\"lighting\":\"LED High-Bay\",\"layout\":\"Double-sided\",\"doorsSideA\":12,\"doorsSideB\":6}"),
                YardConfig = J("{\"totalSpots\":40,\"zones\":[{\"id\":\"A\",\"name\":\"Lot A\",\"spots\":12,\"type\":\"Staging\"}]}")
            });
        }

        if (!db.Accounts.Any())
        {
            db.Accounts.Add(new Account
            {
                Id = "ACCT-1001", Name = "Greenfield Logistics LLC", Type = "Shipper",
                Address = "2840 N Orange Blossom Trl", Address2 = "Suite 200", City = "Orlando", State = "FL", Zip = "32804", Country = "US",
                MainPhone = "(407) 555-0122", Fax = "(407) 555-0123", Email = "dispatch@greenfield.com", Contact = "Maria Gutierrez",
                HoursOpen = "06:00 AM", HoursClose = "06:00 PM", AppointmentReq = false, LiftGateReq = false, DefaultBillTo = "ACCT-1001",
                Lat = 28.5697, Lng = -81.3877, RequiresSignedDocs = true, SignedDocType = "Customer BOL", SignedDocCopies = 2, SignedDocReturn = "Scan OK",
                Notes = "Loading dock accessible from rear lot.",
                EdiSets = J("[\"204\",\"990\",\"214\",\"210\"]"),
                Invoicing = J("{\"billingMethod\":\"FB && Images\",\"paymentTerms\":\"Net 30\",\"dueDays\":15}"),
                NotifPrefs = J("{\"sms\":true,\"email\":true,\"phone\":false,\"triggers\":[\"pickup\",\"in_transit\",\"delivered\"]}")
            });
        }

        if (!db.Drivers.Any())
        {
            db.Drivers.Add(new Driver
            {
                Id = "DRV-2001", Name = "Marcus Johnson", FirstName = "Marcus", LastName = "Johnson", Pin = "2001",
                Status = "On Duty", Photo = "https://randomuser.me/api/portraits/men/32.jpg", PhotoColor = "#3b82f6",
                Cell = "(407) 555-1002", Email = "m.johnson@aurorasoftware.com", CurrentLocation = "I-75 near Valdosta, GA",
                DefaultEquip = "Tractor #1002 / Dry Van #2002", AssignedEquip = "Tractor #1002 / Dry Van #2002", CurrentOrder = "ORL-10018",
                CdlClass = "A", CdlNumber = "J429-118-72-401", CdlState = "FL", CdlExpiry = "08/15/2027", MedCertExpiry = "12/05/2026",
                HomeTerminal = "ORL", TerminalCode = "ORL", Capacity = 44000, AvailableCapacity = 5800, HireDate = "03/15/2022", Dob = "06/22/1988",
                Address = "1420 Magnolia Ave", City = "Orlando", State = "FL", Zip = "32803", EmergencyContact = "Lisa Johnson", EmergencyPhone = "(407) 555-1003",
                PayType = "Per Mile", PayRate = 0.58m, HosStatus = "Driving", HosClock = "6:22 remaining", HosViolations = 0, Mvr = "Clean",
                TwicCard = true, Hazmat = false, AccidentCount = 0, Notes = "Senior driver. Preferred for long-haul SE corridor routes.",
                Endorsements = J("[\"T\",\"N\"]"), Restrictions = J("[]")
            });
        }

        if (!db.Users.Any())
        {
            db.Users.Add(new AppUser
            {
                Id = "ADM-001", Name = "James Donovan", Email = "j.donovan@lykescartage.com", Role = "Company Admin", Dept = "Executive",
                IsAdmin = true, Initials = "JD", Phone = "(407) 555-0001", Status = "Active", InviteStatus = "Accepted",
                Permissions = J("[\"Full Access\",\"Admin\",\"Reports\",\"Billing\",\"Setup\"]"),
                Modules = J("[\"ops\",\"dockops\",\"dispatch\",\"orders\",\"accounts\",\"drivers\",\"terminals\",\"billing\",\"edi\",\"reports\",\"setup\"]"),
                TerminalAccess = J("[\"ALL\"]")
            });
        }

        if (!db.Orders.Any())
        {
            db.Orders.Add(new Order
            {
                Id = "ORL-10018", Customer = "Greenfield Logistics", Origin = "Orlando, FL", Dest = "Atlanta, GA",
                Driver = "Marcus Johnson", Equipment = "Dry Van #1042", Status = "In Transit", Priority = "High",
                OrderDate = "04/03/2026", Pickup = "04/05/2026", Delivery = "04/06/2026", Weight = "38,200", Rate = "$2,450.00",
                Bol = "BOL-88291034", ShipType = "Truckload", Billing = "Prepaid", RefNum = "GF-20260227-A",
                Shipper = J("{\"name\":\"Greenfield Logistics LLC\",\"address\":\"2840 N Orange Blossom Trl\",\"city\":\"Orlando\",\"state\":\"FL\",\"zip\":\"32804\",\"phone\":\"(407) 555-0122\"}"),
                Consignee = J("{\"name\":\"Southeast Distribution Hub\",\"address\":\"1500 Peachtree Industrial Blvd\",\"city\":\"Atlanta\",\"state\":\"GA\",\"zip\":\"30309\",\"phone\":\"(404) 555-0187\"}"),
                BillTo = J("{\"name\":\"Greenfield Logistics LLC\",\"address\":\"2840 N Orange Blossom Trl\",\"city\":\"Orlando\",\"state\":\"FL\",\"zip\":\"32804\",\"phone\":\"(407) 555-0100\"}"),
                LineItems = new List<OrderLineItem>
                {
                    new() { Cls = "65", Sku = "BWS-2400", Desc = "Sparkling Water 24pk", Wt = "1080", Cube = "33.0", Pieces = 60, Unit = "pcs" },
                    new() { Cls = "85", Sku = "POO-1200", Desc = "Premium Olive Oil 1L x12", Wt = "576", Cube = "19.2", Pieces = 48, Unit = "pcs" },
                    new() { Cls = "65", Sku = "PGN-5000", Desc = "Napkins Case 500ct", Wt = "400", Cube = "24.0", Pieces = 40, Unit = "pcs" }
                }
            });
        }

        db.SaveChanges();
    }
}
