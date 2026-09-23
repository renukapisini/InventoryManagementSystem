using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<CategoryViewModel?> GetCategoryByIdAsync(int id);
        Task<(bool Success, string Message)> CreateCategoryAsync(CategoryViewModel model, string currentUsername);
        Task<(bool Success, string Message)> UpdateCategoryAsync(CategoryViewModel model, string currentUsername);
        Task<(bool Success, string Message)> ToggleCategoryStatusAsync(int id, string currentUsername);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id, string currentUsername);
    }

    public interface IUserService
    {
        Task<UserListViewModel> GetUsersAsync(string? searchTerm, int? roleId, bool? isActive, string? department);
        Task<UserViewModel?> GetUserByIdAsync(int id);
        Task<UserViewModel?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
        Task<(bool Success, string Message, int UserId)> CreateUserAsync(UserViewModel model, string currentUsername);
        Task<(bool Success, string Message)> UpdateUserAsync(UserViewModel model, string currentUsername);
        Task<(bool Success, string Message)> ToggleUserActiveAsync(int id, string currentUsername);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string currentUsername);
        Task<UserProfileViewModel?> GetUserProfileAsync(int userId);
        Task<UserViewModel> PrepareUserViewModelAsync(UserViewModel? model = null);
    }
}
