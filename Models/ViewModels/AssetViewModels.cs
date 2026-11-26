using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class AssetViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asset Code is mandatory.")]
        [StringLength(50, ErrorMessage = "Asset Code cannot exceed 50 characters.")]
        [Display(Name = "Asset Code")]
        public string AssetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Asset Name is mandatory.")]
        [StringLength(150, ErrorMessage = "Asset Name cannot exceed 150 characters.")]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is mandatory.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "Purchase Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Purchase Cost is required.")]
        [Range(0, 10000000, ErrorMessage = "Purchase Cost must be a non-negative number.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Purchase Cost ($)")]
        public decimal PurchaseCost { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 100000, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [StringLength(100)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [StringLength(100)]
        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Assigned User")]
        public int? AssignedToUserId { get; set; }

        public string? AssignedToUserName { get; set; }

        [Required(ErrorMessage = "Status is mandatory.")]
        [Display(Name = "Status")]
        public string Status { get; set; } = AssetStatus.Available;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Select lists for dropdowns
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
    }

    public class AssetListViewModel
    {
        public List<AssetViewModel> Assets { get; set; } = new List<AssetViewModel>();
        public string? SearchTerm { get; set; }
        public int? CategoryIdFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string? LocationFilter { get; set; }
        public string? DepartmentFilter { get; set; }
        public int? AssignedUserIdFilter { get; set; }

        // Dropdown SelectLists
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> Departments { get; set; } = new List<string>();

        // Pagination
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
