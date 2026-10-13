using System.ComponentModel.DataAnnotations;

namespace LocalJobPortal.Models
{
    public class JobApplicationViewModel
    {
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Cover Message")]
        public string CoverMessage { get; set; } = string.Empty;
    }
}