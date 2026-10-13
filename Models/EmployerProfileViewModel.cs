using System.ComponentModel.DataAnnotations;

namespace LocalJobPortal.Models
{
    public class EmployerProfileViewModel
    {
        public int EmployerId { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;
    }
}