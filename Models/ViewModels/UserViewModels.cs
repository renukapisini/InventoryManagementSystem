using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee ID is mandatory.")]
        [StringLength(50)]
        [Display(Name = "Employee ID")]
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
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        [Required(ErrorMessage = "Username is mandatory.")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character.")]
        [Display(Name = "Password")]
        public string? Password { get; set; } // Required only on create

        [Required(ErrorMessage = "Role is mandatory.")]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int AssignedAssetsCount { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }

    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public string? SearchTerm { get; set; }
        public int? RoleIdFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
        public string? DepartmentFilter { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
        public List<string> Departments { get; set; } = new List<string>();
    }
}
