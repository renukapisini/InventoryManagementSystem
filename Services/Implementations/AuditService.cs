using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActivityAsync(string action, string entityName, int? entityId, string performedBy, string description)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? "System" : performedBy,
                    Description = description,
                    CreatedDate = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Audit Log created: [{Action}] {EntityName} #{EntityId} by {User}", action, entityName, entityId, performedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log for [{Action}] {EntityName}", action, entityName);
            }
        }

        public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int limit = 20)
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetFilteredAuditLogsAsync(DateTime? startDate, DateTime? endDate, string? action, string? entityName, string? performedBy)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.CreatedDate <= endOfDay);
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(a => a.Action.ToLower() == action.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                query = query.Where(a => a.EntityName.ToLower() == entityName.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(performedBy))
            {
                query = query.Where(a => a.PerformedBy.ToLower().Contains(performedBy.ToLower()));
            }

            return await query
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }
    }
}
