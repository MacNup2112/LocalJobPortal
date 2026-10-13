namespace LocalJobPortal.Models
{
    public class ManageJobViewModel
    {
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string Salary { get; set; } = string.Empty;

        public string RequiredSkills { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int TotalVacancies { get; set; }

        public int FilledVacancies { get; set; }

        public int RemainingVacancies =>
            Math.Max(0, TotalVacancies - FilledVacancies);
    }
}