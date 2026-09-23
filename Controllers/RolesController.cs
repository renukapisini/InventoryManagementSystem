using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Asset Manager")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetRolesAsync();
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditPermissions(int id)
        {
            var model = await _roleService.GetRoleForEditPermissionsAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPermissions(EditRolePermissionsViewModel model)
        {
            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _roleService.UpdateRolePermissionsAsync(model, currentUsername);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
