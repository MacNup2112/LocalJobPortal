using System.ComponentModel.DataAnnotations;

namespace LocalJobPortal.Models
{
    public class JobSeekerProfileViewModel
    {
        public int JobSeekerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Skills { get; set; } = string.Empty;
    }
}