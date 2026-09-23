using InventoryManagementSystem.Models.Entities;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        // KPI Cards
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int AssignedAssets { get; set; }
        public int UnderMaintenanceAssets { get; set; }
        public int RetiredAssets { get; set; }
        public int DamagedAssets { get; set; }
        public int LostAssets { get; set; }
        public int TotalCategories { get; set; }
        public int UnreadNotifications { get; set; }
        public decimal TotalInventoryValue { get; set; }

        // Recent Data Lists
        public List<AssetViewModel> RecentAssets { get; set; } = new List<AssetViewModel>();
        public List<AuditLog> RecentActivities { get; set; } = new List<AuditLog>();
        public List<Notification> RecentNotifications { get; set; } = new List<Notification>();

        // Chart Data Maps
        public Dictionary<string, int> AssetsByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByLocation { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByDepartment { get; set; } = new Dictionary<string, int>();
    }
}
