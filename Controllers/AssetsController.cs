using InventoryManagementSystem.Authorization;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? categoryId,
            string? status,
            string? location,
            string? department,
            int? assignedUserId,
            int pageIndex = 1)
        {
            var model = await _assetService.GetAssetsAsync(searchTerm, categoryId, status, location, department, assignedUserId, pageIndex);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Asset not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _assetService.PrepareAssetViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _assetService.PrepareAssetViewModelAsync(model);
                return View(model);
            }

            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _assetService.CreateAssetAsync(model, currentUsername);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model = await _assetService.PrepareAssetViewModelAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.AssetId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Asset not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid Asset request.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                model = await _assetService.PrepareAssetViewModelAsync(model);
                return View(model);
            }

            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _assetService.UpdateAssetAsync(model, currentUsername);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model = await _assetService.PrepareAssetViewModelAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _assetService.DeleteAssetAsync(id, currentUsername);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
