using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is mandatory.")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public int AssetCount { get; set; }
    }

    public class RoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int UserCount { get; set; }
        public List<string> AssignedPermissions { get; set; } = new List<string>();
    }

    public class EditRolePermissionsViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public List<PermissionSelectionViewModel> Permissions { get; set; } = new List<PermissionSelectionViewModel>();
    }

    public class PermissionSelectionViewModel
    {
        public int PermissionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "General";
        public bool IsSelected { get; set; }
    }

    public class OrganizationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Organisation Name is mandatory.")]
        [StringLength(150)]
        [Display(Name = "Organisation Name")]
        public string OrganisationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organisation Code is mandatory.")]
        [StringLength(50)]
        [Display(Name = "Organisation Code")]
        public string OrganisationCode { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Url]
        [StringLength(150)]
        public string? Website { get; set; }

        [Display(Name = "Current Logo Path")]
        public string? LogoPath { get; set; }

        [Display(Name = "Upload New Logo")]
        public IFormFile? LogoFile { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
