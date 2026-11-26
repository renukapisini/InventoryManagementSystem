# Inventory Management System (IMS) — Enterprise ASP.NET Core MVC Application

An enterprise-grade, secure, full-stack web application built in **ASP.NET Core MVC** (.NET 10), **Entity Framework Core (SQLite In-Memory)**, **Bootstrap 5**, and **Chart.js**.

Designed following clean **Layered MVC + Service Layer Architecture**, SOLID principles, dependency injection, role-based authorization, real-time KPI dashboards, in-app notification center, SMTP email dispatch, audit logging, and reporting.

---

## Enhanced Features

1. **User Registration & Validation**
   - New registration module (`/Account/Register`) supporting Full Name, Email, Phone Number, Password, Confirm Password, optional Employee ID, and Role.
   - Validation: Required fields, email format, password complexity (min 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special character), password confirmation, duplicate email, and duplicate employee ID checks.
   - Secure password hashing using ASP.NET Core `IPasswordHasher<User>`.

2. **Email OTP Verification System**
   - 6-digit numeric OTP generated via `IOtpService` upon user registration.
   - Hashed OTP storage with 10-minute expiry time.
   - Single-use policy and rate-limiting (60-second resend cooldown, max 5 attempts per OTP code).
   - Email dispatch via `IEmailService` (SMTP with dev mode fallback logging).

3. **Google OAuth 2.0 / Google Login**
   - "Continue with Google" integration using `Microsoft.AspNetCore.Authentication.Google`.
   - Flow: Login/Register page $\rightarrow$ Google Authorization $\rightarrow$ Backend validates identity $\rightarrow$ Find or create user account in DB $\rightarrow$ Establish authenticated Cookie session $\rightarrow$ Redirect to Dashboard.
   - Stores Google Subject ID (`GoogleSubjectId`), Email, First Name, Last Name.
   - **Mock Google Login** endpoint included for local dev testing without active live secrets.

4. **Asset Creation Notifications & Quantity**
   - Asset model updated with `Quantity` (default: `1`, range: `1..100000`).
   - Creation of an asset automatically triggers in-app notification formatted as:
     `"New asset Dell Laptop (AST-1001) has been added to inventory."`
   - Connected directly to the asset creation database transaction.

5. **Notification UI & Header Bell**
   - Header notification bell with live unread badge count and interactive dropdown preview.
   - Mark individual notification as read & mark all as read.

---

## Demo Accounts (Auto-Seeded)

The application automatically seeds deterministic sample data on startup.

| Role | Username / Email | Password | Scope / Access Level |
| :--- | :--- | :--- | :--- |
| **Asset Manager** | `manager@ims.com` or `manager` | `Password123!` | Full Admin (Assets, Users, Roles, Org, Reports, Notifications) |
| **Inventory Executive** | `executive@ims.com` or `executive` | `Password123!` | Operational (Assets CRUD, Reports, Notifications, Profile) |

*Note: Quick demo login buttons are provided on the Login page.*

---

## Google Cloud & OAuth Setup Instructions

To configure live Google OAuth 2.0 login in Google Cloud Console:

1. Go to the [Google Cloud Console](https://console.cloud.google.com/).
2. Create a new project (e.g. `IMS-Inventory-System`).
3. Navigate to **APIs & Services** $\rightarrow$ **OAuth consent screen**:
   - Select **User Type**: External.
   - Fill in App Information (App Name: `Inventory Management System`, User support email, Developer contact).
   - Save and continue.
4. Navigate to **APIs & Services** $\rightarrow$ **Credentials**:
   - Click **Create Credentials** $\rightarrow$ Select **OAuth client ID**.
   - Application type: **Web application**.
   - Name: `IMS Web Client`.
   - **Authorized JavaScript origins**:
     `https://localhost:5001`, `http://localhost:5000`
   - **Authorized redirect URIs**:
     `https://localhost:5001/signin-google`, `http://localhost:5000/signin-google`
5. Click **Create**. Copy the **Client ID** and **Client Secret**.

### Setting Environment Variables:
Add the credentials to `appsettings.json` or system environment variables:

```json
"Google": {
  "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
  "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
}
```

Or set in PowerShell:
```powershell
$env:Google__ClientId="YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
$env:Google__ClientSecret="YOUR_GOOGLE_CLIENT_SECRET"
```

---

## Build & Execution Instructions

### 1. Run Web Application:
```powershell
cd C:\Users\xxx\.gemini\antigravity\scratch\InventoryManagementSystem
dotnet run
```
Access at `https://localhost:5001` or `http://localhost:5000`.

### 2. Run Automated Tests:
```powershell
cd C:\Users\xxx\.gemini\antigravity\scratch\InventoryManagementSystem.Tests
dotnet test
```

---

## Final SRS Requirements-to-Implementation Checklist

| Requirement | Implementation Status | Notes |
| :--- | :---: | :--- |
| **User Registration** | Completed | Full Name, Email, Phone, Password, Employee ID, Role selection |
| **Google Login** | Completed | Google OAuth 2.0 + Mock Google Login option for dev testing |
| **Email OTP System** | Completed | 6-digit numeric OTP, 10-min expiry, rate-limiting, single-use |
| **Google Cloud Readiness** | Completed | Configured via environment variables / appsettings with docs |
| **Asset Creation Notification** | Completed | `"New asset {AssetName} ({AssetCode}) has been added to inventory."` |
| **Notification Header UI** | Completed | Bell icon with unread count badge & interactive dropdown preview |
| **Asset Quantity** | Completed | Quantity field integrated across entities, ViewModels, and Razor views |
| **Automated Unit Tests** | Completed | 6/6 xUnit test cases passed cleanly |
