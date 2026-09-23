# Inventory Management System (IMS)

An enterprise-grade, secure, full-stack web application built in **ASP.NET Core MVC** (.NET 9 / .NET 10), **Entity Framework Core (SQLite In-Memory)**, **Bootstrap 5**, and **Chart.js**.

Designed following clean **Layered MVC + Service Layer Architecture**, SOLID principles, dependency injection, role-based authorization, real-time KPI dashboards, in-app notification center, SMTP email dispatch, audit logging, and reporting.

---

## Key Features

1. **Role-Based Access Control & Security**
   - Cookie-based authentication (`CookieAuthenticationDefaults.AuthenticationScheme`).
   - Secure password hashing using ASP.NET Core `IPasswordHasher<User>`.
   - Granular permission-based authorization policies (`[HasPermission(...)]`).
   - Two distinct roles:
     - **Asset Manager**: Full administrative access to assets, categories, users, roles, permissions, organisation profile, reports, and audit logs.
     - **Inventory Executive**: Operational access to asset CRUD, reports, notifications, and profile management. (Restricted from User, Role, & Org Administration).
   - Prevents users from modifying their own role or deactivating their own account.
   - Deactivated users are blocked from logging in or having assets assigned.

2. **Inventory Asset Management**
   - Full CRUD operations with server & client validation.
   - **Unique Asset Code** enforcement & duplicate code prevention.
   - Search by asset code, name, serial number, category, location, or assigned custodian.
   - Filter by category, status (Available, Assigned, Under Maintenance, Damaged, Retired, Lost), location, department, and assigned user.
   - Asset assignment restricted to **Active users only**.
   - Confirmation dialogs before deleting assets.

3. **Category Management**
   - Asset Manager exclusive module.
   - Create, edit, toggle active status, and view linked asset counts.
   - **Delete Protection**: Prevents deleting categories that currently have linked assets.

4. **User Management**
   - Asset Manager exclusive module.
   - Create, edit, search, filter, and activate/deactivate users.
   - Unique Username and Email validation.

5. **Organisation Profile & Branding**
   - Customise company name, code, contact details, and website.
   - **Logo Image Upload** supporting PNG, JPG, JPEG, SVG, WebP formats (max 2MB).
   - Organisation logo & name rendered dynamically across top header navbar and generated report titles.

6. **Interactive Dashboard & Chart.js**
   - Live KPI cards (Total Assets, Active Assets, Available Assets, Assigned Assets, Under Maintenance, Damaged/Lost, Retired, Total Categories, Unread Notifications, Total Inventory Value).
   - Real-time Chart.js visualisations:
     - Assets by Category (Doughnut Chart)
     - Assets by Status (Bar Chart)
     - Assets by Location (Pie Chart)
     - Assets by Department (Horizontal Bar Chart)
   - Recent asset updates list, activity feeds, and notification badges.

7. **Notification Center & SMTP Email Service**
   - In-app notification center with read/unread indicators, badge counts, and user isolation.
   - Automatic notification dispatch on Asset Created, Updated, and Deleted events.
   - SMTP Email Service configured via `appsettings.json` with fallback logging in dev mode.

8. **Audit Trail**
   - `AuditLogs` table tracking Action (Create, Update, Delete, Login, PasswordChange), Entity, Entity ID, PerformedBy, Description, and Server Timestamps.

9. **Comprehensive Reporting Module**
   - 6 Filtered Reports:
     1. Inventory Register Report
     2. Asset Status Report
     3. Category Report
     4. Location Report
     5. Department Report
     6. Inventory Activity Log Report
   - Filters by date range, category, status, location, department, and user. Print support integrated.

---

## Demo Accounts (Auto-Seeded)

The application automatically seeds deterministic sample data on startup.

| Role | Username / Email | Password | Scope / Access Level |
| :--- | :--- | :--- | :--- |
| **Asset Manager** | `manager@ims.com` or `manager` | `Password123!` | Full Admin (Assets, Users, Roles, Org, Reports, Notifications) |
| **Inventory Executive** | `executive@ims.com` or `executive` | `Password123!` | Operational (Assets CRUD, Reports, Notifications, Profile) |

*Note: 1-click quick login buttons are provided on the Login page for convenience.*

---

## Technology Stack

- **Backend**: C#, ASP.NET Core 9/10 MVC, Entity Framework Core 10, Dependency Injection.
- **Frontend**: Razor Views (.cshtml), Bootstrap 5, Bootstrap Icons, Chart.js, jQuery Validation.
- **Database**: SQLite In-Memory using a singleton persistent `SqliteConnection`.
- **Testing**: xUnit, Moq, EF Core InMemory.

---

## Project Structure

```
InventoryManagementSystem/
├── Controllers/              # MVC Controllers (Account, Dashboard, Assets, Categories, Users, Roles, Organization, Notifications, Reports, Profile)
├── Models/
│   ├── Entities/             # Domain Entities (User, Role, Permission, RolePermission, Asset, Category, Notification, AuditLog, Organization)
│   └── ViewModels/           # Form & List ViewModels
├── Data/
│   ├── ApplicationDbContext.cs # EF Core DbContext
│   └── DbInitializer.cs       # Seed Data Service (~25 Assets, Users, Roles, Org Profile)
├── Services/
│   ├── Interfaces/           # Service Interfaces (IAssetService, IUserService, ICategoryService, etc.)
│   └── Implementations/      # Business Logic Services
├── Authorization/            # Permission Handler & Attributes ([HasPermission])
├── Views/                    # Razor Views (Shared _Layout, Account, Dashboard, Assets, Categories, Users, Roles, Organization, Notifications, Reports, Profile)
├── wwwroot/
│   ├── css/site.css          # Custom Responsive Design System
│   └── js/dashboard-charts.js # Chart.js Visualization Logic
├── Program.cs                # Startup, DI & Middleware Setup
├── appsettings.json          # Configuration Settings
└── README.md                 # Project Documentation

InventoryManagementSystem.Tests/
├── UnitTest1.cs              # Automated xUnit Business Logic Tests
└── InventoryManagementSystem.Tests.csproj
```

---

## Build & Execution Instructions

### 1. Run the Web Application
Open PowerShell / Terminal in the project directory:
```powershell
cd C:\Users\sridivya\.gemini\antigravity\scratch\InventoryManagementSystem
dotnet run
```
Open your browser at `https://localhost:5001` or `http://localhost:5000`.

### 2. Execute Automated Unit Tests
```powershell
cd C:\Users\sridivya\.gemini\antigravity\scratch\InventoryManagementSystem.Tests
dotnet test
```

---

## Database Behavior & Persistence

The application uses **EF Core SQLite In-Memory** configured with a persistent `SqliteConnection` kept open for the lifetime of the process in `Program.cs`:
```csharp
var sqliteConnection = new SqliteConnection("Data Source=InMemoryImsDb;Mode=Memory;Cache=Shared");
sqliteConnection.Open();
```
- **In-Memory Data Retention**: All created, updated, or deleted assets, users, and audit logs persist in memory throughout the running application session.
- **Reset Behavior**: Stopping or restarting the application process clears the in-memory database, which is then cleanly re-seeded by `DbInitializer.cs`.

---

## SMTP Email Configuration

To configure real SMTP email delivery, update `appsettings.json`:
```json
"SmtpSettings": {
  "Host": "smtp.yourprovider.com",
  "Port": 587,
  "EnableSsl": true,
  "SenderEmail": "notifications@yourcompany.com",
  "SenderName": "IMS Notifications",
  "Username": "your_smtp_username",
  "Password": "your_smtp_password",
  "IsEnabled": true
}
```
If `IsEnabled` is `false`, the email service operates in **Development Mode**, logging dispatched email contents safely to the console without interrupting workflow execution.

---

## Final SRS Requirements-to-Implementation Checklist

| Requirement | Implementation Status | Notes |
| :--- | :---: | :--- |
| **ASP.NET Core MVC Layered Architecture** | Completed | Controllers, Service Layer, EF Core Data Layer, ViewModels, Razor Views |
| **SQLite In-Memory Connection** | Completed | Persistent singleton SqliteConnection setup in `Program.cs` |
| **Authentication & Password Hashing** | Completed | Cookie Auth, `IPasswordHasher<User>` password hashing |
| **Role-Based & Permission Authorization** | Completed | `[Authorize]`, `[HasPermission]`, Asset Manager & Executive roles |
| **Asset Management CRUD** | Completed | Unique code, search, filter, pagination, active user check |
| **Category Management** | Completed | CRUD, active toggle, delete protection if assets linked |
| **User Management** | Completed | Asset Manager only, unique username/email, activate/deactivate |
| **Role & Permission Matrix** | Completed | Manage permissions assigned to roles |
| **Organisation Profile & Logo Upload** | Completed | Edit profile, logo file upload (PNG/JPG/SVG/WebP, max 2MB) |
| **Dashboard KPIs & Charts** | Completed | 8 KPI cards, 4 Chart.js charts (Category, Status, Location, Dept) |
| **In-App Notification Center** | Completed | Mark read, unread count badge, user notification isolation |
| **SMTP Email Service** | Completed | Email dispatch on Asset Create/Update/Delete with dev fallback |
| **Audit Trail** | Completed | `AuditLogs` table recording user actions and server timestamps |
| **6 Filtered Reports** | Completed | Register, Status, Category, Location, Department, Activity reports |
| **My Profile & Change Password** | Completed | View profile, change password with complexity validation |
| **Automated Testing** | Completed | xUnit test suite passing 100% (5/5 tests passed) |
