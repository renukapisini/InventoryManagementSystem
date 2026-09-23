using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Models.ViewModels
{
    public class ReportFilterViewModel
    {
        public string ReportType { get; set; } = "Register"; // Register, Status, Category, Location, Department, Activity

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Assigned User")]
        public int? AssignedUserId { get; set; }

        // Filter Options
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> Departments { get; set; } = new List<string>();
    }

    public class ReportResultViewModel
    {
        public string Title { get; set; } = "Inventory Report";
        public string OrganisationName { get; set; } = "Acme Global Solutions";
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public ReportFilterViewModel Filter { get; set; } = new ReportFilterViewModel();
        public int TotalCount { get; set; }

        // Report Data Lists
        public List<AssetViewModel> InventoryRegister { get; set; } = new List<AssetViewModel>();
        public List<GroupedReportItem> GroupedItems { get; set; } = new List<GroupedReportItem>();
        public List<AuditLog> ActivityLogs { get; set; } = new List<AuditLog>();
    }

    public class GroupedReportItem
    {
        public string GroupName { get; set; } = string.Empty;
        public int TotalAssets { get; set; }
        public decimal TotalValue { get; set; }
        public List<AssetViewModel> Assets { get; set; } = new List<AssetViewModel>();
    }
}
