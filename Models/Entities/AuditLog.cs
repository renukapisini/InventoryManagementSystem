using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, PasswordChange

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // Asset, User, Category, Role, Organization

        public int? EntityId { get; set; }

        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
