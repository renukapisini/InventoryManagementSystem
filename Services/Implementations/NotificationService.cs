using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(int userId, string message, string type, int? entityId = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    NotificationType = type,
                    EntityId = entityId,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create in-app notification for User #{UserId}", userId);
            }
        }

        public async Task NotifyAllManagersAsync(string message, string type, int? entityId = null)
        {
            try
            {
                var managerUserIds = await _context.Users
                    .Where(u => u.Role != null && u.Role.Name == "Asset Manager" && u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var managerId in managerUserIds)
                {
                    await CreateNotificationAsync(managerId, message, type, entityId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify managers of type {Type}", type);
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int limit = 20)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var n in unreadNotifications)
                {
                    n.IsRead = true;
                }
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
