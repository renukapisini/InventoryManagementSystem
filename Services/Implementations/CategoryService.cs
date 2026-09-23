using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ApplicationDbContext context, IAuditService auditService, ILogger<CategoryService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    AssetCount = c.Assets.Count
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return null;

            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                AssetCount = category.Assets.Count
            };
        }

        public async Task<(bool Success, string Message)> CreateCategoryAsync(CategoryViewModel model, string currentUsername)
        {
            var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == model.Name.Trim().ToLower());
            if (exists)
            {
                return (false, $"A category named '{model.Name}' already exists.");
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync("Create", "Category", category.Id, currentUsername, $"Created category '{category.Name}'.");
            return (true, $"Category '{category.Name}' created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(CategoryViewModel model, string currentUsername)
        {
            var category = await _context.Categories.FindAsync(model.Id);
            if (category == null)
            {
                return (false, "Category not found.");
            }

            var duplicate = await _context.Categories.AnyAsync(c => c.Id != model.Id && c.Name.ToLower() == model.Name.Trim().ToLower());
            if (duplicate)
            {
                return (false, $"Another category named '{model.Name}' already exists.");
            }

            category.Name = model.Name.Trim();
            category.Description = model.Description?.Trim();
            category.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync("Update", "Category", category.Id, currentUsername, $"Updated category '{category.Name}'.");
            return (true, $"Category '{category.Name}' updated successfully.");
        }

        public async Task<(bool Success, string Message)> ToggleCategoryStatusAsync(int id, string currentUsername)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return (false, "Category not found.");
            }

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            var statusStr = category.IsActive ? "Activated" : "Deactivated";
            await _auditService.LogActivityAsync("Update", "Category", category.Id, currentUsername, $"{statusStr} category '{category.Name}'.");

            return (true, $"Category '{category.Name}' has been {statusStr.ToLower()}.");
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id, string currentUsername)
        {
            var category = await _context.Categories
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return (false, "Category not found.");
            }

            if (category.Assets.Any())
            {
                return (false, $"Cannot delete category '{category.Name}' because it has {category.Assets.Count} linked assets. Reassign or remove assets first.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync("Delete", "Category", id, currentUsername, $"Deleted category '{category.Name}'.");
            return (true, $"Category '{category.Name}' deleted successfully.");
        }
    }
}
