using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Asset Manager")]
    public class OrganizationController : Controller
    {
        private readonly IOrganizationService _orgService;
        private readonly IWebHostEnvironment _env;

        public OrganizationController(IOrganizationService orgService, IWebHostEnvironment env)
        {
            _orgService = orgService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _orgService.GetOrganizationProfileAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var model = await _orgService.GetOrganizationProfileAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganizationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _orgService.UpdateOrganizationProfileAsync(model, _env.WebRootPath, currentUsername);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
