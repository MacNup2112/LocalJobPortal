namespace LocalJobPortal.Models
{
    public class ApplicantViewModel
    {
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string CoverMessage { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime AppliedDate { get; set; }
    }
}