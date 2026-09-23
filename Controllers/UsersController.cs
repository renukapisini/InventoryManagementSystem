using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Asset Manager")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int? roleId, bool? isActive, string? department)
        {
            var model = await _userService.GetUsersAsync(searchTerm, roleId, isActive, department);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User account not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _userService.PrepareUserViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _userService.PrepareUserViewModelAsync(model);
                return View(model);
            }

            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _userService.CreateUserAsync(model, currentUsername);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model = await _userService.PrepareUserViewModelAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.UserId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User account not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid User request.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                model = await _userService.PrepareUserViewModelAsync(model);
                return View(model);
            }

            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _userService.UpdateUserAsync(model, currentUsername);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model = await _userService.PrepareUserViewModelAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _userService.ToggleUserActiveAsync(id, currentUsername);

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
