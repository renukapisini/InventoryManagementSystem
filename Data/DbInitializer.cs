using InventoryManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if already seeded
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            var hasher = new PasswordHasher<User>();

            // 1. Seed Roles
            var roleManager = new Role
            {
                Name = "Asset Manager",
                Description = "Full administrative access to all assets, users, roles, reports, and organisation settings.",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            };

            var roleExecutive = new Role
            {
                Name = "Inventory Executive",
                Description = "Operational access to view, create, edit, delete assets, generate reports, and view notifications.",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            };

            context.Roles.AddRange(roleManager, roleExecutive);
            context.SaveChanges();

            // 2. Seed Permissions
            var permissions = new List<Permission>
            {
                new Permission { Code = "VIEW_ASSET", Name = "View Assets", Description = "View asset list and details", Category = "Inventory" },
                new Permission { Code = "CREATE_ASSET", Name = "Create Asset", Description = "Create new inventory asset", Category = "Inventory" },
                new Permission { Code = "EDIT_ASSET", Name = "Edit Asset", Description = "Modify existing inventory asset", Category = "Inventory" },
                new Permission { Code = "DELETE_ASSET", Name = "Delete Asset", Description = "Remove inventory asset", Category = "Inventory" },
                new Permission { Code = "VIEW_REPORT", Name = "View Reports", Description = "Generate and view inventory reports", Category = "Reports" },
                new Permission { Code = "MANAGE_USERS", Name = "Manage Users", Description = "Create, edit, activate, deactivate users", Category = "Administration" },
                new Permission { Code = "MANAGE_ROLES", Name = "Manage Roles", Description = "Manage roles and permissions", Category = "Administration" },
                new Permission { Code = "MANAGE_PERMISSIONS", Name = "Manage Permissions", Description = "Assign permissions to roles", Category = "Administration" },
                new Permission { Code = "MANAGE_ORGANIZATION", Name = "Manage Organisation", Description = "Update organisation profile and logo", Category = "Administration" },
                new Permission { Code = "VIEW_NOTIFICATIONS", Name = "View Notifications", Description = "View in-app notifications", Category = "General" },
            };

            context.Permissions.AddRange(permissions);
            context.SaveChanges();

            // 3. Map Role Permissions
            // Asset Manager gets all permissions
            foreach (var perm in permissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleManager.Id,
                    PermissionId = perm.Id
                });
            }

            // Inventory Executive gets operational permissions
            var execPermCodes = new[] { "VIEW_ASSET", "CREATE_ASSET", "EDIT_ASSET", "DELETE_ASSET", "VIEW_REPORT", "VIEW_NOTIFICATIONS" };
            foreach (var perm in permissions.Where(p => execPermCodes.Contains(p.Code)))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleExecutive.Id,
                    PermissionId = perm.Id
                });
            }
            context.SaveChanges();

            // 4. Seed Users
            var adminUser = new User
            {
                EmployeeId = "EMP001",
                FirstName = "Eleanor",
                LastName = "Vane",
                Email = "manager@ims.com",
                MobileNumber = "+1 (555) 019-1001",
                Username = "manager",
                RoleId = roleManager.Id,
                Department = "IT Operations",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Password123!");

            var execUser = new User
            {
                EmployeeId = "EMP002",
                FirstName = "Marcus",
                LastName = "Brody",
                Email = "executive@ims.com",
                MobileNumber = "+1 (555) 019-1002",
                Username = "executive",
                RoleId = roleExecutive.Id,
                Department = "Logistics",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-25)
            };
            execUser.PasswordHash = hasher.HashPassword(execUser, "Password123!");

            var staffUser1 = new User
            {
                EmployeeId = "EMP003",
                FirstName = "Sophia",
                LastName = "Chen",
                Email = "sophia.chen@ims.com",
                MobileNumber = "+1 (555) 019-1003",
                Username = "schen",
                RoleId = roleExecutive.Id,
                Department = "Software Engineering",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-20)
            };
            staffUser1.PasswordHash = hasher.HashPassword(staffUser1, "Password123!");

            var staffUser2 = new User
            {
                EmployeeId = "EMP004",
                FirstName = "David",
                LastName = "Miller",
                Email = "david.miller@ims.com",
                MobileNumber = "+1 (555) 019-1004",
                Username = "dmiller",
                RoleId = roleExecutive.Id,
                Department = "Human Resources",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-15)
            };
            staffUser2.PasswordHash = hasher.HashPassword(staffUser2, "Password123!");

            var inactiveUser = new User
            {
                EmployeeId = "EMP005",
                FirstName = "Robert",
                LastName = "Taylor",
                Email = "robert.taylor@ims.com",
                MobileNumber = "+1 (555) 019-1005",
                Username = "rtaylor",
                RoleId = roleExecutive.Id,
                Department = "Finance",
                IsActive = false,
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            };
            inactiveUser.PasswordHash = hasher.HashPassword(inactiveUser, "Password123!");

            context.Users.AddRange(adminUser, execUser, staffUser1, staffUser2, inactiveUser);
            context.SaveChanges();

            // 5. Seed Categories
            var categories = new List<Category>
            {
                new Category { Name = "Laptop", Description = "Portable laptops, ultrabooks, and mobile workstations", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
                new Category { Name = "Desktop", Description = "Desktop PCs, all-in-ones, and workstation towers", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
                new Category { Name = "Printer", Description = "Laser, inkjet, and multifunction office printers", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
                new Category { Name = "Monitor", Description = "External computer displays and monitors", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
                new Category { Name = "Networking", Description = "Switches, routers, access points, and firewalls", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
                new Category { Name = "Furniture", Description = "Ergonomic chairs, standing desks, and conference tables", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
                new Category { Name = "Mobile", Description = "Smartphones, test tablets, and mobile devices", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
                new Category { Name = "Other", Description = "Peripherals, accessories, cable kits, and adapters", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-30) }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            var catMap = categories.ToDictionary(c => c.Name, c => c.Id);

            // 6. Seed Organisation Profile
            var org = new Organization
            {
                OrganisationName = "Apex Global Enterprise",
                OrganisationCode = "ORG-APEX-01",
                Address = "500 Innovation Boulevard, Suite 800",
                City = "San Francisco",
                State = "CA",
                Country = "USA",
                PostalCode = "94107",
                Phone = "+1 (800) 555-0199",
                Email = "info@apexenterprise.com",
                Website = "https://www.apexenterprise.com",
                LogoPath = "/images/default-logo.svg",
                UpdatedBy = adminUser.Username,
                UpdatedDate = DateTime.UtcNow.AddDays(-5)
            };
            context.Organizations.Add(org);
            context.SaveChanges();

            // 7. Seed Sample Assets (~25 realistic assets)
            var assets = new List<Asset>
            {
                new Asset {
                    AssetCode = "AST-LAP-001",
                    AssetName = "Apple MacBook Pro 16\" M3 Max",
                    CategoryId = catMap["Laptop"],
                    Description = "36GB RAM, 1TB SSD Space Black for Principal Architect",
                    SerialNumber = "C02G1234MD6R",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-6),
                    PurchaseCost = 3499.00m,
                    Location = "Building A - Floor 4",
                    Department = "Software Engineering",
                    AssignedToUserId = staffUser1.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new Asset {
                    AssetCode = "AST-LAP-002",
                    AssetName = "Dell XPS 15 9530",
                    CategoryId = catMap["Laptop"],
                    Description = "Core i9, 32GB RAM, 1TB SSD, RTX 4060",
                    SerialNumber = "889XPS2024",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-5),
                    PurchaseCost = 2299.50m,
                    Location = "Building B - Floor 2",
                    Department = "Software Engineering",
                    AssignedToUserId = execUser.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new Asset {
                    AssetCode = "AST-LAP-003",
                    AssetName = "Lenovo ThinkPad X1 Carbon Gen 11",
                    CategoryId = catMap["Laptop"],
                    Description = "Core i7, 16GB RAM, 512GB SSD, Ultralight",
                    SerialNumber = "PF4592X1",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-4),
                    PurchaseCost = 1850.00m,
                    Location = "Building A - Floor 3",
                    Department = "Human Resources",
                    AssignedToUserId = staffUser2.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-4)
                },
                new Asset {
                    AssetCode = "AST-LAP-004",
                    AssetName = "HP EliteBook 840 G10",
                    CategoryId = catMap["Laptop"],
                    Description = "Core i5, 16GB RAM, 512GB SSD",
                    SerialNumber = "5CG3041999",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-3),
                    PurchaseCost = 1350.00m,
                    Location = "IT Storage Room",
                    Department = "IT Operations",
                    AssignedToUserId = null,
                    Status = AssetStatus.Available,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-3)
                },
                new Asset {
                    AssetCode = "AST-LAP-005",
                    AssetName = "Asus ROG Zephyrus G16",
                    CategoryId = catMap["Laptop"],
                    Description = "High performance graphics workstation",
                    SerialNumber = "N9R0CX009",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-2),
                    PurchaseCost = 2499.00m,
                    Location = "Service Center",
                    Department = "Software Engineering",
                    AssignedToUserId = null,
                    Status = AssetStatus.UnderMaintenance,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-2)
                },
                new Asset {
                    AssetCode = "AST-DESK-001",
                    AssetName = "Apple iMac 24\" M3 Blue",
                    CategoryId = catMap["Desktop"],
                    Description = "16GB RAM, 512GB SSD, Retina 4.5K Display",
                    SerialNumber = "C02M3IMAC24",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-8),
                    PurchaseCost = 1699.00m,
                    Location = "Building A - Floor 1",
                    Department = "Marketing",
                    AssignedToUserId = adminUser.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-8)
                },
                new Asset {
                    AssetCode = "AST-DESK-002",
                    AssetName = "Dell OptiPlex 7090 Tower",
                    CategoryId = catMap["Desktop"],
                    Description = "Core i7, 32GB RAM, 1TB NVMe, Dual Display Output",
                    SerialNumber = "DP7090TW001",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-12),
                    PurchaseCost = 1250.00m,
                    Location = "Building B - Floor 1",
                    Department = "Finance",
                    AssignedToUserId = null,
                    Status = AssetStatus.Available,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-12)
                },
                new Asset {
                    AssetCode = "AST-DESK-003",
                    AssetName = "HP Z2 Tower G9 Workstation",
                    CategoryId = catMap["Desktop"],
                    Description = "Intel Xeon, 64GB ECC RAM, RTX A4000 GPU",
                    SerialNumber = "HPZ2WORK99",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-10),
                    PurchaseCost = 3100.00m,
                    Location = "Building A - Lab 2",
                    Department = "Software Engineering",
                    AssignedToUserId = null,
                    Status = AssetStatus.Available,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-10)
                },
                new Asset {
                    AssetCode = "AST-MON-001",
                    AssetName = "Dell UltraSharp U2723QE 27\" 4K USB-C",
                    CategoryId = catMap["Monitor"],
                    Description = "IPS Black Panel, 4K UHD, USB-C Hub 90W PD",
                    SerialNumber = "CN-0U2723-7444",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-6),
                    PurchaseCost = 580.00m,
                    Location = "Building A - Floor 4 Desk 402",
                    Department = "Software Engineering",
                    AssignedToUserId = staffUser1.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new Asset {
                    AssetCode = "AST-MON-002",
                    AssetName = "Dell UltraSharp U2723QE 27\" 4K USB-C",
                    CategoryId = catMap["Monitor"],
                    Description = "IPS Black Panel, 4K UHD, USB-C Hub 90W PD",
                    SerialNumber = "CN-0U2723-7445",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-6),
                    PurchaseCost = 580.00m,
                    Location = "Building A - Floor 4 Desk 402",
                    Department = "Software Engineering",
                    AssignedToUserId = staffUser1.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new Asset {
                    AssetCode = "AST-MON-003",
                    AssetName = "LG 34WN80C-B UltraWide Curved 34\"",
                    CategoryId = catMap["Monitor"],
                    Description = "WQHD IPS USB-C Curved Monitor",
                    SerialNumber = "LG34UW8091",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-7),
                    PurchaseCost = 650.00m,
                    Location = "Building B - Floor 2 Desk 210",
                    Department = "Logistics",
                    AssignedToUserId = execUser.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-7)
                },
                new Asset {
                    AssetCode = "AST-PRN-001",
                    AssetName = "HP LaserJet Enterprise M507x",
                    CategoryId = catMap["Printer"],
                    Description = "Heavy duty monochrome laser printer with auto duplex",
                    SerialNumber = "PHPRN507X01",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-14),
                    PurchaseCost = 890.00m,
                    Location = "Building A - Floor 3 Print Bay",
                    Department = "Human Resources",
                    AssignedToUserId = null,
                    Status = AssetStatus.Available,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-14)
                },
                new Asset {
                    AssetCode = "AST-PRN-002",
                    AssetName = "Canon ImageCLASS MF743Cdw",
                    CategoryId = catMap["Printer"],
                    Description = "Wireless Color All-in-One Laser Printer",
                    SerialNumber = "CANMF743-881",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-11),
                    PurchaseCost = 550.00m,
                    Location = "Building B - Floor 1 Hallway",
                    Department = "Finance",
                    AssignedToUserId = null,
                    Status = AssetStatus.Damaged,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-11)
                },
                new Asset {
                    AssetCode = "AST-NET-001",
                    AssetName = "Cisco Catalyst 9300 48-Port Switch",
                    CategoryId = catMap["Networking"],
                    Description = "48 Port UPOE+, Network Essentials Gigabit Switch",
                    SerialNumber = "FCW2431C09300",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-18),
                    PurchaseCost = 4200.00m,
                    Location = "Main Server Room Rack A",
                    Department = "IT Operations",
                    AssignedToUserId = adminUser.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-18)
                },
                new Asset {
                    AssetCode = "AST-NET-002",
                    AssetName = "Ubiquiti UniFi Dream Machine Special Edition",
                    CategoryId = catMap["Networking"],
                    Description = "10G SFP+ Enterprise Security Gateway & PoE Switch",
                    SerialNumber = "UDM-SE-90812",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-9),
                    PurchaseCost = 499.00m,
                    Location = "Building B Server Closet",
                    Department = "IT Operations",
                    AssignedToUserId = null,
                    Status = AssetStatus.Available,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-9)
                },
                new Asset {
                    AssetCode = "AST-NET-003",
                    AssetName = "Aruba AP-535 Wi-Fi 6 Access Point",
                    CategoryId = catMap["Networking"],
                    Description = "Enterprise Dual-Radio 802.11ax AP",
                    SerialNumber = "ARUAP535-0012",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-15),
                    PurchaseCost = 950.00m,
                    Location = "Building A - Floor 4 Ceiling",
                    Department = "IT Operations",
                    AssignedToUserId = null,
                    Status = AssetStatus.Available,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-15)
                },
                new Asset {
                    AssetCode = "AST-FUR-001",
                    AssetName = "Herman Miller Aeron Ergonomic Chair Size B",
                    CategoryId = catMap["Furniture"],
                    Description = "Fully adjustable arms, PostureFit SL, Mineral finish",
                    SerialNumber = "HM-AERON-2023-01",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-12),
                    PurchaseCost = 1495.00m,
                    Location = "Executive Suite 1",
                    Department = "IT Operations",
                    AssignedToUserId = adminUser.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-12)
                },
                new Asset {
                    AssetCode = "AST-FUR-002",
                    AssetName = "Steelcase Gesture Desk Chair",
                    CategoryId = catMap["Furniture"],
                    Description = "Dark shell, 3D knit back, Lumbar support",
                    SerialNumber = "ST-GEST-8812",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-10),
                    PurchaseCost = 1350.00m,
                    Location = "Building A - Floor 4 Desk 402",
                    Department = "Software Engineering",
                    AssignedToUserId = staffUser1.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-10)
                },
                new Asset {
                    AssetCode = "AST-FUR-003",
                    AssetName = "UPLIFT V2 Commercial Standing Desk 60x30",
                    CategoryId = catMap["Furniture"],
                    Description = "Dual motor electric sit-stand desk, Walnut top",
                    SerialNumber = "UPLIFT-6030-W",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-8),
                    PurchaseCost = 850.00m,
                    Location = "Building A - Floor 4 Desk 402",
                    Department = "Software Engineering",
                    AssignedToUserId = staffUser1.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-8)
                },
                new Asset {
                    AssetCode = "AST-MOB-001",
                    AssetName = "Apple iPhone 15 Pro 256GB Natural Titanium",
                    CategoryId = catMap["Mobile"],
                    Description = "Company test phone for Mobile App QA",
                    SerialNumber = "F2LX15PRO256",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-4),
                    PurchaseCost = 1099.00m,
                    Location = "QA Mobile Testing Lab",
                    Department = "Software Engineering",
                    AssignedToUserId = execUser.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-4)
                },
                new Asset {
                    AssetCode = "AST-MOB-002",
                    AssetName = "Samsung Galaxy S24 Ultra 512GB Titanium Gray",
                    CategoryId = catMap["Mobile"],
                    Description = "Company executive mobile device",
                    SerialNumber = "R58M10S24ULT",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-3),
                    PurchaseCost = 1299.00m,
                    Location = "Building B - Floor 2",
                    Department = "Logistics",
                    AssignedToUserId = execUser.Id,
                    Status = AssetStatus.Assigned,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-3)
                },
                new Asset {
                    AssetCode = "AST-OTH-001",
                    AssetName = "Logitech MX Master 3S Wireless Mouse",
                    CategoryId = catMap["Other"],
                    Description = "Quiet Click, 8K DPI Sensor, Pale Gray",
                    SerialNumber = "LOGMX3S-901",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-2),
                    PurchaseCost = 99.99m,
                    Location = "IT Desk Supply Station",
                    Department = "IT Operations",
                    AssignedToUserId = null,
                    Status = AssetStatus.Available,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-2)
                },
                new Asset {
                    AssetCode = "AST-OTH-002",
                    AssetName = "Anker 737 Power Bank 24,000mAh 140W",
                    CategoryId = catMap["Other"],
                    Description = "High output Portable Charger for Laptops",
                    SerialNumber = "ANK737PB-140",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-5),
                    PurchaseCost = 149.99m,
                    Location = "IT Storage Room",
                    Department = "IT Operations",
                    AssignedToUserId = null,
                    Status = AssetStatus.Retired,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new Asset {
                    AssetCode = "AST-OTH-003",
                    AssetName = "Apple iPad Air 11\" M2 128GB Wi-Fi Space Gray",
                    CategoryId = catMap["Other"],
                    Description = "Demonstration tablet for reception desk check-in",
                    SerialNumber = "DMPM2IPADAIR",
                    PurchaseDate = DateTime.UtcNow.AddMonths(-1),
                    PurchaseCost = 599.00m,
                    Location = "Building A Reception",
                    Department = "Human Resources",
                    AssignedToUserId = null,
                    Status = AssetStatus.Lost,
                    CreatedBy = adminUser.Username,
                    CreatedDate = DateTime.UtcNow.AddMonths(-1)
                }
            };

            context.Assets.AddRange(assets);
            context.SaveChanges();

            // 8. Seed Initial Notifications
            var notifications = new List<Notification>
            {
                new Notification
                {
                    UserId = adminUser.Id,
                    Message = "System database initialized with sample inventory records and roles.",
                    NotificationType = "System",
                    EntityId = null,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow.AddDays(-1)
                },
                new Notification
                {
                    UserId = execUser.Id,
                    Message = "New Asset Assigned: Apple MacBook Pro 16\" (AST-LAP-001)",
                    NotificationType = "AssetAssigned",
                    EntityId = assets[0].Id,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow.AddHours(-12)
                },
                new Notification
                {
                    UserId = execUser.Id,
                    Message = "Welcome to IMS! Your Inventory Executive account is active.",
                    NotificationType = "System",
                    EntityId = null,
                    IsRead = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-2)
                }
            };

            context.Notifications.AddRange(notifications);
            context.SaveChanges();

            // 9. Seed Audit Logs
            var auditLogs = new List<AuditLog>
            {
                new AuditLog
                {
                    Action = "Create",
                    EntityName = "Asset",
                    EntityId = assets[0].Id,
                    PerformedBy = adminUser.Username,
                    Description = $"Created asset '{assets[0].AssetName}' ({assets[0].AssetCode}) with purchase cost ${assets[0].PurchaseCost:N2}.",
                    CreatedDate = DateTime.UtcNow.AddMonths(-6)
                },
                new AuditLog
                {
                    Action = "Create",
                    EntityName = "User",
                    EntityId = execUser.Id,
                    PerformedBy = adminUser.Username,
                    Description = $"Created user '{execUser.Username}' ({execUser.Email}) with role '{roleExecutive.Name}'.",
                    CreatedDate = DateTime.UtcNow.AddDays(-25)
                },
                new AuditLog
                {
                    Action = "Update",
                    EntityName = "Asset",
                    EntityId = assets[4].Id,
                    PerformedBy = adminUser.Username,
                    Description = $"Updated status for asset '{assets[4].AssetCode}' to '{AssetStatus.UnderMaintenance}'.",
                    CreatedDate = DateTime.UtcNow.AddDays(-2)
                }
            };

            context.AuditLogs.AddRange(auditLogs);
            context.SaveChanges();
        }
    }
}
