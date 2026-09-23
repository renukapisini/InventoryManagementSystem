using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface IAssetService
    {
        Task<AssetListViewModel> GetAssetsAsync(string? searchTerm, int? categoryId, string? status, string? location, string? department, int? assignedUserId, int pageIndex = 1, int pageSize = 10);
        Task<AssetViewModel?> GetAssetByIdAsync(int id);
        Task<AssetViewModel?> GetAssetByCodeAsync(string code);
        Task<bool> IsAssetCodeUniqueAsync(string code, int? excludeId = null);
        Task<(bool Success, string Message, int AssetId)> CreateAssetAsync(AssetViewModel model, string currentUsername);
        Task<(bool Success, string Message)> UpdateAssetAsync(AssetViewModel model, string currentUsername);
        Task<(bool Success, string Message)> DeleteAssetAsync(int id, string currentUsername);
        Task<AssetViewModel> PrepareAssetViewModelAsync(AssetViewModel? model = null);
    }
}
