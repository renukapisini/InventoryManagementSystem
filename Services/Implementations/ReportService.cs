using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrganizationService _orgService;

        public ReportService(ApplicationDbContext context, IOrganizationService orgService)
        {
            _context = context;
            _orgService = orgService;
        }

        public async Task<ReportResultViewModel> GenerateReportAsync(ReportFilterViewModel filter)
        {
            var org = await _orgService.GetOrganizationProfileAsync();

            // Populate filter select options
            var categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            var users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FirstName).ToListAsync();
            var locations = await _context.Assets.Where(a => !string.IsNullOrEmpty(a.Location)).Select(a => a.Location!).Distinct().OrderBy(l => l).ToListAsync();
            var departments = await _context.Assets.Where(a => !string.IsNullOrEmpty(a.Department)).Select(a => a.Department!).Distinct().OrderBy(d => d).ToListAsync();

            filter.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = c.Id == filter.CategoryId });
            filter.Statuses = AssetStatus.AllStatuses.Select(s => new SelectListItem { Value = s, Text = s, Selected = s == filter.Status });
            filter.Users = users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName, Selected = u.Id == filter.AssignedUserId });
            filter.Locations = locations;
            filter.Departments = departments;

            var result = new ReportResultViewModel
            {
                OrganisationName = org.OrganisationName,
                GeneratedDate = DateTime.Now,
                Filter = filter
            };

            if (filter.ReportType == "Activity")
            {
                result.Title = "Inventory Activity Report";
                var query = _context.AuditLogs.AsQueryable();

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(a => a.CreatedDate >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(a => a.CreatedDate <= endOfDay);
                }

                result.ActivityLogs = await query.OrderByDescending(a => a.CreatedDate).ToListAsync();
                result.TotalCount = result.ActivityLogs.Count;
                return result;
            }

            // Asset queries for other 5 reports
            var assetQuery = _context.Assets
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            if (filter.StartDate.HasValue)
            {
                assetQuery = assetQuery.Where(a => a.PurchaseDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                assetQuery = assetQuery.Where(a => a.PurchaseDate <= endOfDay);
            }

            if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            {
                assetQuery = assetQuery.Where(a => a.CategoryId == filter.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                assetQuery = assetQuery.Where(a => a.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                assetQuery = assetQuery.Where(a => a.Location == filter.Location);
            }

            if (!string.IsNullOrWhiteSpace(filter.Department))
            {
                assetQuery = assetQuery.Where(a => a.Department == filter.Department);
            }

            if (filter.AssignedUserId.HasValue && filter.AssignedUserId.Value > 0)
            {
                assetQuery = assetQuery.Where(a => a.AssignedToUserId == filter.AssignedUserId.Value);
            }

            var assetList = await assetQuery
                .OrderBy(a => a.AssetCode)
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
                    Status = a.Status
                })
                .ToListAsync();

            result.TotalCount = assetList.Count;

            switch (filter.ReportType)
            {
                case "Status":
                    result.Title = "Asset Status Report";
                    result.GroupedItems = assetList
                        .GroupBy(a => a.Status)
                        .Select(g => new GroupedReportItem
                        {
                            GroupName = g.Key,
                            TotalAssets = g.Count(),
                            TotalValue = g.Sum(x => x.PurchaseCost),
                            Assets = g.ToList()
                        }).OrderBy(g => g.GroupName).ToList();
                    break;

                case "Category":
                    result.Title = "Category Report";
                    result.GroupedItems = assetList
                        .GroupBy(a => a.CategoryName ?? "Uncategorized")
                        .Select(g => new GroupedReportItem
                        {
                            GroupName = g.Key,
                            TotalAssets = g.Count(),
                            TotalValue = g.Sum(x => x.PurchaseCost),
                            Assets = g.ToList()
                        }).OrderBy(g => g.GroupName).ToList();
                    break;

                case "Location":
                    result.Title = "Location Report";
                    result.GroupedItems = assetList
                        .GroupBy(a => string.IsNullOrEmpty(a.Location) ? "Unspecified Location" : a.Location)
                        .Select(g => new GroupedReportItem
                        {
                            GroupName = g.Key,
                            TotalAssets = g.Count(),
                            TotalValue = g.Sum(x => x.PurchaseCost),
                            Assets = g.ToList()
                        }).OrderBy(g => g.GroupName).ToList();
                    break;

                case "Department":
                    result.Title = "Department Report";
                    result.GroupedItems = assetList
                        .GroupBy(a => string.IsNullOrEmpty(a.Department) ? "Unassigned Department" : a.Department)
                        .Select(g => new GroupedReportItem
                        {
                            GroupName = g.Key,
                            TotalAssets = g.Count(),
                            TotalValue = g.Sum(x => x.PurchaseCost),
                            Assets = g.ToList()
                        }).OrderBy(g => g.GroupName).ToList();
                    break;

                case "Register":
                default:
                    result.Title = "Inventory Register Report";
                    result.InventoryRegister = assetList;
                    break;
            }

            return result;
        }
    }
}
