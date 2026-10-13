using System.ComponentModel.DataAnnotations;

namespace LocalJobPortal.Models
{
    public class EditJobViewModel
    {
        public int JobId { get; set; }

        [Required]
        [StringLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string JobType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Salary { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string RequiredSkills { get; set; } = string.Empty;

        [Required]
        public string JobDescription { get; set; } = string.Empty;
    }
}