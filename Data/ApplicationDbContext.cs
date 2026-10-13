using LocalJobPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalJobPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employer> Employers { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<JobSeeker> JobSeekers { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Admin> Admins { get; set; }
    }
}