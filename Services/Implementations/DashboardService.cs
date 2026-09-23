using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardMetricsAsync(int currentUserId, bool isManager)
        {
            var assets = await _context.Assets
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .ToListAsync();

            var totalAssets = assets.Count;
            var availableAssets = assets.Count(a => a.Status == AssetStatus.Available);
            var assignedAssets = assets.Count(a => a.Status == AssetStatus.Assigned);
            var underMaintenanceAssets = assets.Count(a => a.Status == AssetStatus.UnderMaintenance);
            var damagedAssets = assets.Count(a => a.Status == AssetStatus.Damaged);
            var retiredAssets = assets.Count(a => a.Status == AssetStatus.Retired);
            var lostAssets = assets.Count(a => a.Status == AssetStatus.Lost);

            var activeAssets = availableAssets + assignedAssets + underMaintenanceAssets;
            var totalCategories = await _context.Categories.CountAsync(c => c.IsActive);
            var unreadNotifications = await _context.Notifications.CountAsync(n => n.UserId == currentUserId && !n.IsRead);
            var totalValue = assets.Sum(a => a.PurchaseCost);

            // Recent Lists
            var recentAssets = assets
                .OrderByDescending(a => a.UpdatedDate ?? a.CreatedDate)
                .Take(5)
                .Select(a => new AssetViewModel
                {
                    Id = a.Id,
                    AssetCode = a.AssetCode,
                    AssetName = a.AssetName,
                    CategoryName = a.Category?.Name ?? "N/A",
                    Status = a.Status,
                    Location = a.Location,
                    PurchaseCost = a.PurchaseCost,
                    CreatedDate = a.CreatedDate
                }).ToList();

            var recentActivities = await _context.AuditLogs
                .OrderByDescending(a => a.CreatedDate)
                .Take(5)
                .ToListAsync();

            var recentNotifications = await _context.Notifications
                .Where(n => n.UserId == currentUserId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToListAsync();

            // Chart Aggregations
            var byCategory = assets
                .GroupBy(a => a.Category?.Name ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Count());

            var byStatus = assets
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var byLocation = assets
                .Where(a => !string.IsNullOrEmpty(a.Location))
                .GroupBy(a => a.Location!)
                .ToDictionary(g => g.Key, g => g.Count());

            var byDepartment = assets
                .Where(a => !string.IsNullOrEmpty(a.Department))
                .GroupBy(a => a.Department!)
                .ToDictionary(g => g.Key, g => g.Count());

            return new DashboardViewModel
            {
                TotalAssets = totalAssets,
                ActiveAssets = activeAssets,
                AvailableAssets = availableAssets,
                AssignedAssets = assignedAssets,
                UnderMaintenanceAssets = underMaintenanceAssets,
                DamagedAssets = damagedAssets,
                RetiredAssets = retiredAssets,
                LostAssets = lostAssets,
                TotalCategories = totalCategories,
                UnreadNotifications = unreadNotifications,
                TotalInventoryValue = totalValue,
                RecentAssets = recentAssets,
                RecentActivities = recentActivities,
                RecentNotifications = recentNotifications,
                AssetsByCategory = byCategory,
                AssetsByStatus = byStatus,
                AssetsByLocation = byLocation,
                AssetsByDepartment = byDepartment
            };
        }
    }
}
