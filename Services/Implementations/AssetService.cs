using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AssetService> _logger;

        public AssetService(
            ApplicationDbContext context,
            IAuditService auditService,
            INotificationService notificationService,
            IEmailService emailService,
            ILogger<AssetService> logger)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AssetListViewModel> GetAssetsAsync(
            string? searchTerm,
            int? categoryId,
            string? status,
            string? location,
            string? department,
            int? assignedUserId,
            int pageIndex = 1,
            int pageSize = 10)
        {
            var query = _context.Assets
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(a =>
                    a.AssetCode.ToLower().Contains(term) ||
                    a.AssetName.ToLower().Contains(term) ||
                    (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(term)) ||
                    (a.Category != null && a.Category.Name.ToLower().Contains(term)) ||
                    (a.Location != null && a.Location.ToLower().Contains(term)) ||
                    (a.AssignedToUser != null && (a.AssignedToUser.FirstName.ToLower().Contains(term) || a.AssignedToUser.LastName.ToLower().Contains(term))));
            }

            // Dropdown filters
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(a => a.Location == location);
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(a => a.Department == department);
            }

            if (assignedUserId.HasValue && assignedUserId.Value > 0)
            {
                query = query.Where(a => a.AssignedToUserId == assignedUserId.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (pageIndex < 1) pageIndex = 1;
            if (pageIndex > totalPages) pageIndex = totalPages;

            var items = await query
                .OrderByDescending(a => a.UpdatedDate ?? a.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssetViewModel
                {
                    Id = a.Id,
                    AssetCode = a.AssetCode,
                    AssetName = a.AssetName,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category != null ? a.Category.Name : "N/A",
                    Description = a.Description,
                    SerialNumber = a.SerialNumber,
                    PurchaseDate = a.PurchaseDate,
                    PurchaseCost = a.PurchaseCost,
                    Location = a.Location,
                    Department = a.Department,
                    AssignedToUserId = a.AssignedToUserId,
                    AssignedToUserName = a.AssignedToUser != null ? a.AssignedToUser.FullName : "Unassigned",
                    Status = a.Status,
                    CreatedBy = a.CreatedBy,
                    CreatedDate = a.CreatedDate,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedDate = a.UpdatedDate
                })
                .ToListAsync();

            var categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            var users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FirstName).ToListAsync();
            var locations = await _context.Assets.Where(a => !string.IsNullOrEmpty(a.Location)).Select(a => a.Location!).Distinct().OrderBy(l => l).ToListAsync();
            var departments = await _context.Assets.Where(a => !string.IsNullOrEmpty(a.Department)).Select(a => a.Department!).Distinct().OrderBy(d => d).ToListAsync();

            return new AssetListViewModel
            {
                Assets = items,
                SearchTerm = searchTerm,
                CategoryIdFilter = categoryId,
                StatusFilter = status,
                LocationFilter = location,
                DepartmentFilter = department,
                AssignedUserIdFilter = assignedUserId,
                PageIndex = pageIndex,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = c.Id == categoryId }),
                Statuses = AssetStatus.AllStatuses.Select(s => new SelectListItem { Value = s, Text = s, Selected = s == status }),
                Users = users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.FullName} ({u.Department ?? "No Dept"})", Selected = u.Id == assignedUserId }),
                Locations = locations,
                Departments = departments
            };
        }

        public async Task<AssetViewModel?> GetAssetByIdAsync(int id)
        {
            var a = await _context.Assets
                .Include(asset => asset.Category)
                .Include(asset => asset.AssignedToUser)
                .FirstOrDefaultAsync(asset => asset.Id == id);

            if (a == null) return null;

            var model = new AssetViewModel
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name,
                Description = a.Description,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchaseCost = a.PurchaseCost,
                Location = a.Location,
                Department = a.Department,
                AssignedToUserId = a.AssignedToUserId,
                AssignedToUserName = a.AssignedToUser?.FullName,
                Status = a.Status,
                CreatedBy = a.CreatedBy,
                CreatedDate = a.CreatedDate,
                UpdatedBy = a.UpdatedBy,
                UpdatedDate = a.UpdatedDate
            };

            return await PrepareAssetViewModelAsync(model);
        }

        public async Task<AssetViewModel?> GetAssetByCodeAsync(string code)
        {
            var a = await _context.Assets
                .Include(asset => asset.Category)
                .Include(asset => asset.AssignedToUser)
                .FirstOrDefaultAsync(asset => asset.AssetCode.ToLower() == code.Trim().ToLower());

            if (a == null) return null;

            return new AssetViewModel
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name,
                Description = a.Description,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchaseCost = a.PurchaseCost,
                Location = a.Location,
                Department = a.Department,
                AssignedToUserId = a.AssignedToUserId,
                AssignedToUserName = a.AssignedToUser?.FullName,
                Status = a.Status
            };
        }

        public async Task<bool> IsAssetCodeUniqueAsync(string code, int? excludeId = null)
        {
            var query = _context.Assets.Where(a => a.AssetCode.ToLower() == code.Trim().ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<(bool Success, string Message, int AssetId)> CreateAssetAsync(AssetViewModel model, string currentUsername)
        {
            // Business Rule: Asset Code mandatory & unique
            if (!await IsAssetCodeUniqueAsync(model.AssetCode))
            {
                return (false, $"Asset Code '{model.AssetCode}' already exists in the system. Asset Code must be unique.", 0);
            }

            // Business Rule: Category mandatory & active
            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null || !category.IsActive)
            {
                return (false, "Selected category is invalid or inactive.", 0);
            }

            // Business Rule: Purchase cost non-negative
            if (model.PurchaseCost < 0)
            {
                return (false, "Purchase cost cannot be negative.", 0);
            }

            // Business Rule: Assigned user active check
            if (model.AssignedToUserId.HasValue && model.AssignedToUserId.Value > 0)
            {
                var assignedUser = await _context.Users.FindAsync(model.AssignedToUserId.Value);
                if (assignedUser == null || !assignedUser.IsActive)
                {
                    return (false, "Cannot assign asset to an inactive or non-existent user.", 0);
                }
            }
            else
            {
                model.AssignedToUserId = null;
            }

            var asset = new Asset
            {
                AssetCode = model.AssetCode.Trim().ToUpper(),
                AssetName = model.AssetName.Trim(),
                CategoryId = model.CategoryId,
                Description = model.Description?.Trim(),
                SerialNumber = model.SerialNumber?.Trim(),
                PurchaseDate = model.PurchaseDate,
                PurchaseCost = model.PurchaseCost,
                Location = model.Location?.Trim(),
                Department = model.Department?.Trim(),
                AssignedToUserId = model.AssignedToUserId,
                Status = model.Status,
                CreatedBy = currentUsername,
                CreatedDate = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            // Audit Trail
            await _auditService.LogActivityAsync("Create", "Asset", asset.Id, currentUsername,
                $"Created asset '{asset.AssetName}' ({asset.AssetCode}) with purchase cost ${asset.PurchaseCost:N2}.");

            // In-app Notification
            var notifMsg = $"New Inventory Added: {asset.AssetName} ({asset.AssetCode}) by {currentUsername}.";
            await _notificationService.NotifyAllManagersAsync(notifMsg, "AssetCreated", asset.Id);

            if (asset.AssignedToUserId.HasValue)
            {
                await _notificationService.CreateNotificationAsync(asset.AssignedToUserId.Value,
                    $"Asset Assigned: You have been assigned asset '{asset.AssetName}' ({asset.AssetCode}).", "AssetAssigned", asset.Id);
            }

            // Email Notification (per SRS)
            var currentU = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == currentUsername.ToLower());
            var emailBody = $@"<h3>New Inventory Added</h3>
                <p><strong>Asset Code:</strong> {asset.AssetCode}</p>
                <p><strong>Asset Name:</strong> {asset.AssetName}</p>
                <p><strong>Category:</strong> {category.Name}</p>
                <p><strong>Status:</strong> {asset.Status}</p>
                <p><strong>Created By:</strong> {currentUsername}</p>
                <p><strong>Date/Time:</strong> {DateTime.Now:F}</p>";

            await _emailService.SendEmailNotificationAsync(currentU?.Email ?? "manager@ims.com", "New Inventory Added", emailBody);

            return (true, $"Asset '{asset.AssetName}' ({asset.AssetCode}) created successfully.", asset.Id);
        }

        public async Task<(bool Success, string Message)> UpdateAssetAsync(AssetViewModel model, string currentUsername)
        {
            var asset = await _context.Assets.FindAsync(model.Id);
            if (asset == null)
            {
                return (false, "Asset not found.");
            }

            if (!await IsAssetCodeUniqueAsync(model.AssetCode, model.Id))
            {
                return (false, $"Asset Code '{model.AssetCode}' is already assigned to another asset.");
            }

            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null || !category.IsActive)
            {
                return (false, "Selected category is invalid or inactive.");
            }

            if (model.PurchaseCost < 0)
            {
                return (false, "Purchase cost cannot be negative.");
            }

            if (model.AssignedToUserId.HasValue && model.AssignedToUserId.Value > 0)
            {
                var assignedUser = await _context.Users.FindAsync(model.AssignedToUserId.Value);
                if (assignedUser == null || !assignedUser.IsActive)
                {
                    return (false, "Cannot assign asset to an inactive or non-existent user.");
                }
            }
            else
            {
                model.AssignedToUserId = null;
            }

            asset.AssetCode = model.AssetCode.Trim().ToUpper();
            asset.AssetName = model.AssetName.Trim();
            asset.CategoryId = model.CategoryId;
            asset.Description = model.Description?.Trim();
            asset.SerialNumber = model.SerialNumber?.Trim();
            asset.PurchaseDate = model.PurchaseDate;
            asset.PurchaseCost = model.PurchaseCost;
            asset.Location = model.Location?.Trim();
            asset.Department = model.Department?.Trim();
            asset.AssignedToUserId = model.AssignedToUserId;
            asset.Status = model.Status;
            asset.UpdatedBy = currentUsername;
            asset.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Audit Trail
            await _auditService.LogActivityAsync("Update", "Asset", asset.Id, currentUsername,
                $"Updated asset '{asset.AssetName}' ({asset.AssetCode}) status to '{asset.Status}'.");

            // Notification
            var notifMsg = $"Inventory Updated: Asset '{asset.AssetName}' ({asset.AssetCode}) modified by {currentUsername}.";
            await _notificationService.NotifyAllManagersAsync(notifMsg, "AssetUpdated", asset.Id);

            // Email Notification
            var currentU = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == currentUsername.ToLower());
            var emailBody = $@"<h3>Inventory Updated</h3>
                <p><strong>Asset Code:</strong> {asset.AssetCode}</p>
                <p><strong>Asset Name:</strong> {asset.AssetName}</p>
                <p><strong>Status:</strong> {asset.Status}</p>
                <p><strong>Updated By:</strong> {currentUsername}</p>
                <p><strong>Date/Time:</strong> {DateTime.Now:F}</p>";

            await _emailService.SendEmailNotificationAsync(currentU?.Email ?? "manager@ims.com", "Inventory Updated", emailBody);

            return (true, $"Asset '{asset.AssetName}' ({asset.AssetCode}) updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAssetAsync(int id, string currentUsername)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return (false, "Asset not found.");
            }

            var code = asset.AssetCode;
            var name = asset.AssetName;

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            // Audit Trail
            await _auditService.LogActivityAsync("Delete", "Asset", id, currentUsername, $"Deleted asset '{name}' ({code}).");

            // Notification
            var notifMsg = $"Inventory Deleted: Asset '{name}' ({code}) removed by {currentUsername}.";
            await _notificationService.NotifyAllManagersAsync(notifMsg, "AssetDeleted", id);

            // Email Notification
            var currentU = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == currentUsername.ToLower());
            var emailBody = $@"<h3>Inventory Deleted</h3>
                <p><strong>Asset Code:</strong> {code}</p>
                <p><strong>Asset Name:</strong> {name}</p>
                <p><strong>Deleted By:</strong> {currentUsername}</p>
                <p><strong>Date/Time:</strong> {DateTime.Now:F}</p>";

            await _emailService.SendEmailNotificationAsync(currentU?.Email ?? "manager@ims.com", "Inventory Deleted", emailBody);

            return (true, $"Asset '{name}' ({code}) was deleted successfully.");
        }

        public async Task<AssetViewModel> PrepareAssetViewModelAsync(AssetViewModel? model = null)
        {
            model ??= new AssetViewModel();
            var categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            var users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FirstName).ToListAsync();

            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            });

            model.Users = users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FullName} ({u.Department ?? "General"})",
                Selected = u.Id == model.AssignedToUserId
            });

            model.Statuses = AssetStatus.AllStatuses.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
                Selected = s == model.Status
            });

            return model;
        }
    }
}
