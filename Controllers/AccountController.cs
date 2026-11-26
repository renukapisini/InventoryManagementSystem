using System.Security.Claims;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAuditService _auditService;
        private readonly IOtpService _otpService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IAuditService auditService,
            IOtpService otpService,
            ILogger<AccountController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _auditService = auditService;
            _otpService = otpService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var term = model.UsernameOrEmail.Trim().ToLower();
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == term || u.Email.ToLower() == term);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact your Asset Manager.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe);
            await _auditService.LogActivityAsync("Login", "User", user.Id, user.Username, $"User '{user.Username}' logged in successfully.");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cleanEmail = model.Email.Trim().ToLower();

            // Duplicate Email Check
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == cleanEmail))
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }

            // Employee ID Duplicate Check if provided
            if (!string.IsNullOrWhiteSpace(model.EmployeeId))
            {
                var cleanEmpId = model.EmployeeId.Trim();
                if (await _context.Users.AnyAsync(u => u.EmployeeId.ToLower() == cleanEmpId.ToLower()))
                {
                    ModelState.AddModelError("EmployeeId", $"Employee ID '{cleanEmpId}' is already registered to another user.");
                    return View(model);
                }
            }

            // Store registration data in TempData/Session for OTP completion step
            var nameParts = model.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : model.FullName.Trim();
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var username = cleanEmail.Split('@')[0];
            int count = 1;
            var baseUsername = username;
            while (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
            {
                username = $"{baseUsername}{count++}";
            }

            TempData["Reg_FirstName"] = firstName;
            TempData["Reg_LastName"] = lastName;
            TempData["Reg_Email"] = cleanEmail;
            TempData["Reg_Mobile"] = model.MobileNumber?.Trim();
            TempData["Reg_EmployeeId"] = string.IsNullOrWhiteSpace(model.EmployeeId) ? $"EMP{Random.Shared.Next(100, 999)}" : model.EmployeeId.Trim();
            TempData["Reg_Username"] = username;
            TempData["Reg_Password"] = model.Password;
            TempData["Reg_RoleId"] = model.RoleId > 0 ? model.RoleId : 2;

            // Generate & Send 6-Digit Email OTP
            var otpResult = await _otpService.GenerateAndSendOtpAsync(cleanEmail, "Registration");
            if (!otpResult.Success)
            {
                ModelState.AddModelError(string.Empty, otpResult.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = $"{otpResult.Message} (Dev Mode Code: {otpResult.OtpCodeForDev})";
            return RedirectToAction("VerifyOtp", new { email = cleanEmail, purpose = "Registration" });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp(string email, string purpose = "Registration")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(Register));
            }

            var model = new VerifyOtpViewModel
            {
                Email = email,
                Purpose = purpose
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var verifyResult = await _otpService.VerifyOtpAsync(model.Email, model.OtpCode, model.Purpose);
            if (!verifyResult.Success)
            {
                ModelState.AddModelError("OtpCode", verifyResult.Message);
                return View(model);
            }

            // OTP Verified! Finalize User Registration
            var email = model.Email.Trim().ToLower();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (existingUser != null)
            {
                existingUser.IsEmailVerified = true;
                await _context.SaveChangesAsync();
                await SignInUserAsync(existingUser, false);
                TempData["SuccessMessage"] = "Email verified successfully! Welcome to Inventory Management System.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Create new user from TempData
            var firstName = TempData["Reg_FirstName"]?.ToString() ?? "User";
            var lastName = TempData["Reg_LastName"]?.ToString() ?? string.Empty;
            var mobile = TempData["Reg_Mobile"]?.ToString();
            var empId = TempData["Reg_EmployeeId"]?.ToString() ?? $"EMP{Random.Shared.Next(100, 999)}";
            var username = TempData["Reg_Username"]?.ToString() ?? email.Split('@')[0];
            var plainPassword = TempData["Reg_Password"]?.ToString() ?? "Password123!";
            int.TryParse(TempData["Reg_RoleId"]?.ToString(), out var roleId);
            if (roleId <= 0) roleId = 2; // Default Inventory Executive

            var newUser = new User
            {
                EmployeeId = empId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                MobileNumber = mobile,
                Username = username,
                RoleId = roleId,
                IsActive = true,
                IsEmailVerified = true,
                CreatedDate = DateTime.UtcNow
            };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, plainPassword);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync("Register", "User", newUser.Id, newUser.Username, $"Registered new user account '{newUser.Username}' ({newUser.Email}) via OTP verification.");

            var fullUser = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstAsync(u => u.Id == newUser.Id);

            await SignInUserAsync(fullUser, false);
            TempData["SuccessMessage"] = "Registration & OTP verification complete! Welcome to IMS.";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email, string purpose = "Registration")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Email address is required.";
                return RedirectToAction(nameof(Register));
            }

            var result = await _otpService.GenerateAndSendOtpAsync(email, purpose);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = $"{result.Message} (Dev Mode Code: {result.OtpCodeForDev})";
            }

            return RedirectToAction(nameof(VerifyOtp), new { email, purpose });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                // Fallback attempt to get external login result
                result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            }

            if (!result.Succeeded || result.Principal == null)
            {
                TempData["ErrorMessage"] = "Google authentication failed. Please try logging in manually or use Mock Google Login.";
                return RedirectToAction(nameof(Login));
            }

            var claims = result.Principal.Claims;
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
            var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "Google";
            var familyName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "User";

            return await ProcessGoogleLoginAsync(googleId, email, givenName, familyName);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MockGoogleLogin(string email = "google.user@ims.com", string name = "Google User")
        {
            var nameParts = name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var givenName = nameParts.Length > 0 ? nameParts[0] : "Google";
            var familyName = nameParts.Length > 1 ? nameParts[1] : "User";
            var googleId = $"google_{Guid.NewGuid():N}";

            return await ProcessGoogleLoginAsync(googleId, email, givenName, familyName);
        }

        private async Task<IActionResult> ProcessGoogleLoginAsync(string googleId, string email, string givenName, string familyName)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Google profile did not contain a valid email address.";
                return RedirectToAction(nameof(Login));
            }

            var cleanEmail = email.Trim().ToLower();
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail || u.GoogleSubjectId == googleId);

            if (user == null)
            {
                // Auto-create user for Google Login
                var username = cleanEmail.Split('@')[0];
                int count = 1;
                var baseUsername = username;
                while (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
                {
                    username = $"{baseUsername}{count++}";
                }

                user = new User
                {
                    EmployeeId = $"GOOG-{Random.Shared.Next(1000, 9999)}",
                    FirstName = givenName,
                    LastName = familyName,
                    Email = cleanEmail,
                    Username = username,
                    RoleId = 2, // Inventory Executive
                    IsActive = true,
                    IsEmailVerified = true,
                    GoogleSubjectId = googleId,
                    CreatedDate = DateTime.UtcNow
                };
                user.PasswordHash = _passwordHasher.HashPassword(user, Guid.NewGuid().ToString("N"));

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _auditService.LogActivityAsync("Register", "User", user.Id, user.Username, $"Registered new user account via Google OAuth ({cleanEmail}).");

                user = await _context.Users
                    .Include(u => u.Role)
                    .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstAsync(u => u.Id == user.Id);
            }
            else
            {
                if (string.IsNullOrEmpty(user.GoogleSubjectId))
                {
                    user.GoogleSubjectId = googleId;
                    user.IsEmailVerified = true;
                    await _context.SaveChangesAsync();
                }
            }

            if (!user.IsActive)
            {
                TempData["ErrorMessage"] = "Your account has been deactivated. Please contact your Asset Manager.";
                return RedirectToAction(nameof(Login));
            }

            await SignInUserAsync(user, true);
            await _auditService.LogActivityAsync("Login", "User", user.Id, user.Username, $"User '{user.Username}' logged in via Google OAuth.");

            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}! Logged in via Google.";
            return RedirectToAction("Index", "Dashboard");
        }

        private async Task SignInUserAsync(User user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
            };

            if (user.Role != null)
            {
                foreach (var rp in user.Role.RolePermissions)
                {
                    if (rp.Permission != null)
                    {
                        claims.Add(new Claim("Permission", rp.Permission.Code));
                    }
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "Unknown";
            await _auditService.LogActivityAsync("Logout", "User", null, username, $"User '{username}' logged out.");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
