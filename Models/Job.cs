using System;
using System.ComponentModel.DataAnnotations;

namespace LocalJobPortal.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        [StringLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string JobType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Salary { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string RequiredSkills { get; set; } = string.Empty;

        [Required]
        public string JobDescription { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int TotalVacancies { get; set; } = 1;

        public int FilledVacancies { get; set; } = 0;

        public int EmployerId { get; set; }

        public string Status { get; set; } = "Active";

        public int RemainingVacancies =>
            Math.Max(0, TotalVacancies - FilledVacancies);
    }
}