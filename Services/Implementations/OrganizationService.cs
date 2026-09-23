using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(ApplicationDbContext context, IAuditService auditService, ILogger<OrganizationService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<OrganizationViewModel> GetOrganizationProfileAsync()
        {
            var org = await _context.Organizations.FirstOrDefaultAsync();
            if (org == null)
            {
                org = new Organization();
                _context.Organizations.Add(org);
                await _context.SaveChangesAsync();
            }

            return new OrganizationViewModel
            {
                Id = org.Id,
                OrganisationName = org.OrganisationName,
                OrganisationCode = org.OrganisationCode,
                Address = org.Address,
                City = org.City,
                State = org.State,
                Country = org.Country,
                PostalCode = org.PostalCode,
                Phone = org.Phone,
                Email = org.Email,
                Website = org.Website,
                LogoPath = org.LogoPath,
                UpdatedBy = org.UpdatedBy,
                UpdatedDate = org.UpdatedDate
            };
        }

        public async Task<(bool Success, string Message)> UpdateOrganizationProfileAsync(OrganizationViewModel model, string wwwRootPath, string currentUsername)
        {
            var org = await _context.Organizations.FirstOrDefaultAsync();
            if (org == null)
            {
                org = new Organization();
                _context.Organizations.Add(org);
            }

            // Handle logo file upload
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
                var extension = Path.GetExtension(model.LogoFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return (false, "Invalid image file format. Supported formats: PNG, JPG, JPEG, SVG, WebP.");
                }

                if (model.LogoFile.Length > 2 * 1024 * 1024) // 2MB limit
                {
                    return (false, "Logo file size cannot exceed 2MB.");
                }

                var uploadsFolder = Path.Combine(wwwRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"logo_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(fileStream);
                }

                org.LogoPath = $"/uploads/{uniqueFileName}";
            }

            org.OrganisationName = model.OrganisationName;
            org.OrganisationCode = model.OrganisationCode;
            org.Address = model.Address;
            org.City = model.City;
            org.State = model.State;
            org.Country = model.Country;
            org.PostalCode = model.PostalCode;
            org.Phone = model.Phone;
            org.Email = model.Email;
            org.Website = model.Website;
            org.UpdatedBy = currentUsername;
            org.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("Update", "Organization", org.Id, currentUsername, $"Updated Organisation Profile: '{org.OrganisationName}' ({org.OrganisationCode}).");

            return (true, "Organisation profile updated successfully.");
        }
    }
}
