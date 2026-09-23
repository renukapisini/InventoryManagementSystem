using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.Entities
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string OrganisationName { get; set; } = "Acme Global Solutions";

        [Required]
        [StringLength(50)]
        public string OrganisationCode { get; set; } = "ORG-ACME";

        [StringLength(250)]
        public string? Address { get; set; } = "100 Tech Park Way, Tech Hub";

        [StringLength(100)]
        public string? City { get; set; } = "San Francisco";

        [StringLength(100)]
        public string? State { get; set; } = "CA";

        [StringLength(100)]
        public string? Country { get; set; } = "USA";

        [StringLength(20)]
        public string? PostalCode { get; set; } = "94105";

        [StringLength(30)]
        public string? Phone { get; set; } = "+1 (555) 019-2831";

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; } = "contact@acmeglobal.com";

        [Url]
        [StringLength(150)]
        public string? Website { get; set; } = "https://www.acmeglobal.com";

        [StringLength(250)]
        public string? LogoPath { get; set; } = "/images/logo-placeholder.png";

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
