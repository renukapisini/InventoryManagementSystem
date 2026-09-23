using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            IAuditService auditService,
            IPasswordHasher<User> passwordHasher,
            ILogger<UserService> logger)
        {
            _context = context;
            _auditService = auditService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<UserListViewModel> GetUsersAsync(string? searchTerm, int? roleId, bool? isActive, string? department)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.AssignedAssets)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    u.Username.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.EmployeeId.ToLower().Contains(term));
            }

            if (roleId.HasValue && roleId.Value > 0)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(u => u.Department == department);
            }

            var users = await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    EmployeeId = u.EmployeeId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Username = u.Username,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : "N/A",
                    Department = u.Department,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    AssignedAssetsCount = u.AssignedAssets.Count
                })
                .ToListAsync();

            var roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            var departments = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Department))
                .Select(u => u.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return new UserListViewModel
            {
                Users = users,
                SearchTerm = searchTerm,
                RoleIdFilter = roleId,
                IsActiveFilter = isActive,
                DepartmentFilter = department,
                Roles = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name }),
                Departments = departments
            };
        }

        public async Task<UserViewModel?> GetUserByIdAsync(int id)
        {
            var u = await _context.Users
                .Include(user => user.Role)
                .Include(user => user.AssignedAssets)
                .FirstOrDefaultAsync(user => user.Id == id);

            if (u == null) return null;

            var model = new UserViewModel
            {
                Id = u.Id,
                EmployeeId = u.EmployeeId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name,
                Department = u.Department,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                UpdatedDate = u.UpdatedDate,
                AssignedAssetsCount = u.AssignedAssets.Count
            };

            return await PrepareUserViewModelAsync(model);
        }

        public async Task<UserViewModel?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var term = usernameOrEmail.Trim().ToLower();
            var u = await _context.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Username.ToLower() == term || user.Email.ToLower() == term);

            if (u == null) return null;

            return new UserViewModel
            {
                Id = u.Id,
                EmployeeId = u.EmployeeId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name,
                Department = u.Department,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate
            };
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null)
        {
            var query = _context.Users.Where(u => u.Username.ToLower() == username.Trim().ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            var query = _context.Users.Where(u => u.Email.ToLower() == email.Trim().ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<(bool Success, string Message, int UserId)> CreateUserAsync(UserViewModel model, string currentUsername)
        {
            if (!await IsUsernameUniqueAsync(model.Username))
            {
                return (false, $"Username '{model.Username}' is already taken.", 0);
            }

            if (!await IsEmailUniqueAsync(model.Email))
            {
                return (false, $"Email '{model.Email}' is already registered.", 0);
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return (false, "Password is required when creating a new user.", 0);
            }

            var role = await _context.Roles.FindAsync(model.RoleId);
            if (role == null)
            {
                return (false, "Invalid Role selection.", 0);
            }

            var user = new User
            {
                EmployeeId = model.EmployeeId.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim(),
                MobileNumber = model.MobileNumber?.Trim(),
                Username = model.Username.Trim(),
                RoleId = model.RoleId,
                Department = model.Department?.Trim(),
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync("Create", "User", user.Id, currentUsername, $"Created user '{user.Username}' ({user.Email}) assigned role '{role.Name}'.");
            return (true, $"User '{user.Username}' created successfully.", user.Id);
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(UserViewModel model, string currentUsername)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return (false, "User not found.");
            }

            if (!await IsUsernameUniqueAsync(model.Username, model.Id))
            {
                return (false, $"Username '{model.Username}' is already in use by another account.");
            }

            if (!await IsEmailUniqueAsync(model.Email, model.Id))
            {
                return (false, $"Email '{model.Email}' is already in use by another account.");
            }

            // Prevent self-role modification or self-deactivation if updating own profile via admin form
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == currentUsername.ToLower());
            if (currentUser != null && currentUser.Id == user.Id)
            {
                if (user.RoleId != model.RoleId)
                {
                    return (false, "Security Policy: You cannot modify your own assigned role.");
                }
                if (!model.IsActive)
                {
                    return (false, "Security Policy: You cannot deactivate your own account.");
                }
            }

            user.EmployeeId = model.EmployeeId.Trim();
            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.Email = model.Email.Trim();
            user.MobileNumber = model.MobileNumber?.Trim();
            user.Username = model.Username.Trim();
            user.RoleId = model.RoleId;
            user.Department = model.Department?.Trim();
            user.IsActive = model.IsActive;
            user.UpdatedDate = DateTime.UtcNow;

            // Optional password update
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync("Update", "User", user.Id, currentUsername, $"Updated user details for '{user.Username}'.");
            return (true, $"User '{user.Username}' updated successfully.");
        }

        public async Task<(bool Success, string Message)> ToggleUserActiveAsync(int id, string currentUsername)
        {
            var user = await _context.Users
                .Include(u => u.AssignedAssets)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return (false, "User not found.");
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == currentUsername.ToLower());
            if (currentUser != null && currentUser.Id == user.Id)
            {
                return (false, "Security Policy: You cannot deactivate your own account.");
            }

            user.IsActive = !user.IsActive;
            user.UpdatedDate = DateTime.UtcNow;

            // If deactivating user, unassign assets per business rule or keep record with warning
            if (!user.IsActive && user.AssignedAssets.Any())
            {
                foreach (var asset in user.AssignedAssets)
                {
                    asset.AssignedToUserId = null;
                    asset.Status = AssetStatus.Available;
                    asset.UpdatedBy = currentUsername;
                    asset.UpdatedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            var statusStr = user.IsActive ? "Activated" : "Deactivated";
            await _auditService.LogActivityAsync("Update", "User", user.Id, currentUsername, $"{statusStr} user '{user.Username}'. Assets unassigned if deactivated.");

            return (true, $"User '{user.Username}' has been {statusStr.ToLower()}.");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string currentUsername)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User account not found.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                return (false, "Current password supplied is incorrect.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("PasswordChange", "User", user.Id, currentUsername, $"Changed password for user '{user.Username}'.");

            return (true, "Password changed successfully.");
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(int userId)
        {
            var u = await _context.Users
                .Include(user => user.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Include(user => user.AssignedAssets)
                .ThenInclude(a => a.Category)
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (u == null) return null;

            var permissions = u.Role?.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission!.Name)
                .Distinct()
                .ToList() ?? new List<string>();

            var assignedAssets = u.AssignedAssets.Select(a => new AssetViewModel
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryName = a.Category?.Name,
                Status = a.Status,
                Location = a.Location,
                PurchaseCost = a.PurchaseCost
            }).ToList();

            return new UserProfileViewModel
            {
                Id = u.Id,
                EmployeeId = u.EmployeeId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                Username = u.Username,
                RoleName = u.Role?.Name ?? "N/A",
                Department = u.Department,
                CreatedDate = u.CreatedDate,
                RolePermissions = permissions,
                AssignedAssets = assignedAssets
            };
        }

        public async Task<UserViewModel> PrepareUserViewModelAsync(UserViewModel? model = null)
        {
            model ??= new UserViewModel();
            var roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            model.Roles = roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name, Selected = r.Id == model.RoleId });
            return model;
        }
    }
}
