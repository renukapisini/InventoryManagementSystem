using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<OrganizationViewModel> GetOrganizationProfileAsync();
        Task<(bool Success, string Message)> UpdateOrganizationProfileAsync(OrganizationViewModel model, string wwwRootPath, string currentUsername);
    }

    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string message, string type, int? entityId = null);
        Task NotifyAllManagersAsync(string message, string type, int? entityId = null);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, int limit = 20);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
    }

    public interface IEmailService
    {
        Task SendEmailNotificationAsync(string recipientEmail, string subject, string bodyMessage);
    }

    public interface IAuditService
    {
        Task LogActivityAsync(string action, string entityName, int? entityId, string performedBy, string description);
        Task<List<AuditLog>> GetRecentAuditLogsAsync(int limit = 20);
        Task<List<AuditLog>> GetFilteredAuditLogsAsync(DateTime? startDate, DateTime? endDate, string? action, string? entityName, string? performedBy);
    }

    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardMetricsAsync(int currentUserId, bool isManager);
    }

    public interface IReportService
    {
        Task<ReportResultViewModel> GenerateReportAsync(ReportFilterViewModel filter);
    }

    public interface IRoleService
    {
        Task<List<RoleViewModel>> GetRolesAsync();
        Task<EditRolePermissionsViewModel?> GetRoleForEditPermissionsAsync(int roleId);
        Task<(bool Success, string Message)> UpdateRolePermissionsAsync(EditRolePermissionsViewModel model, string currentUsername);
    }
}
