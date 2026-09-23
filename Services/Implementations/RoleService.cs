using InventoryManagementSystem.Authorization;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public RoleService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<List<RoleViewModel>> GetRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.Users)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    UserCount = r.Users.Count,
                    AssignedPermissions = r.RolePermissions
                        .Where(rp => rp.Permission != null)
                        .Select(rp => rp.Permission!.Name)
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<EditRolePermissionsViewModel?> GetRoleForEditPermissionsAsync(int roleId)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null) return null;

            var allPermissions = await _context.Permissions.ToListAsync();
            var assignedPermIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            return new EditRolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                RoleDescription = role.Description,
                Permissions = allPermissions.Select(p => new PermissionSelectionViewModel
                {
                    PermissionId = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    IsSelected = assignedPermIds.Contains(p.Id)
                }).OrderBy(p => p.Category).ThenBy(p => p.Name).ToList()
            };
        }

        public async Task<(bool Success, string Message)> UpdateRolePermissionsAsync(EditRolePermissionsViewModel model, string currentUsername)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == model.RoleId);

            if (role == null)
            {
                return (false, "Role not found.");
            }

            // Clear existing permissions
            _context.RolePermissions.RemoveRange(role.RolePermissions);

            // Add selected permissions
            var selectedPermIds = model.Permissions.Where(p => p.IsSelected).Select(p => p.PermissionId).ToList();
            foreach (var permId in selectedPermIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permId
                });
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync("Update", "Role", role.Id, currentUsername, $"Updated permission assignments for role '{role.Name}'.");
            return (true, $"Permissions for role '{role.Name}' updated successfully.");
        }
    }
}
