using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is mandatory.")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is mandatory.")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is mandatory.")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<string> RolePermissions { get; set; } = new List<string>();
        public List<AssetViewModel> AssignedAssets { get; set; } = new List<AssetViewModel>();
    }
}
