using System.ComponentModel.DataAnnotations;

namespace LocalJobPortal.Models
{
    public class JobApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public int JobSeekerId { get; set; }

        [Required]
        public string CoverMessage { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime AppliedDate { get; set; } = DateTime.Now;
    }
}