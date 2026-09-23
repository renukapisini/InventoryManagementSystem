using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models.Entities
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AssetCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string AssetName { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase cost must be non-negative.")]
        public decimal PurchaseCost { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public int? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual User? AssignedToUser { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = AssetStatus.Available;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    public static class AssetStatus
    {
        public const string Available = "Available";
        public const string Assigned = "Assigned";
        public const string UnderMaintenance = "Under Maintenance";
        public const string Damaged = "Damaged";
        public const string Retired = "Retired";
        public const string Lost = "Lost";

        public static readonly List<string> AllStatuses = new List<string>()
        {
            Available, Assigned, UnderMaintenance, Damaged, Retired, Lost
        };
    }
}
