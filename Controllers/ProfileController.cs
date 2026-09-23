using System.Security.Claims;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                TempData["ErrorMessage"] = "Profile not found.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var currentUserId) || currentUserId != model.Id)
            {
                TempData["ErrorMessage"] = "Security Exception: Unauthorized profile update attempt.";
                return RedirectToAction(nameof(Index));
            }

            var existingUser = await _userService.GetUserByIdAsync(currentUserId);
            if (existingUser == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Copy allowed profile fields, preserving role, username, and employee ID
            existingUser.FirstName = model.FirstName;
            existingUser.LastName = model.LastName;
            existingUser.Email = model.Email;
            existingUser.MobileNumber = model.MobileNumber;

            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _userService.UpdateUserAsync(existingUser, currentUsername);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUsername = User.Identity?.Name ?? "Unknown";
            var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword, currentUsername);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
