
using LocalJobPortal.Data;
using LocalJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalJobPortal.Controllers
{
    public class JobSeekerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobSeekerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER - GET
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =========================
        // REGISTER - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            JobSeekerRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingJobSeeker = await _context.JobSeekers
                .AnyAsync(j => j.Email == model.Email);

            if (existingJobSeeker)
            {
                ModelState.AddModelError(
                    "Email",
                    "A job seeker with this email already exists.");

                return View(model);
            }

            var jobSeeker = new JobSeeker
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Location = model.Location,
                Skills = model.Skills,
                Password = model.Password
            };

            _context.JobSeekers.Add(jobSeeker);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(RegisterSuccess));
        }

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        // =========================
        // LOGIN - GET
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // LOGIN - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            JobSeekerLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j =>
                    j.Email == model.Email &&
                    j.Password == model.Password);

            if (jobSeeker == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            HttpContext.Session.SetInt32(
                "JobSeekerId",
                jobSeeker.JobSeekerId);

            return RedirectToAction(nameof(Dashboard));
        }

        // =========================
        // DASHBOARD
        // =========================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var jobSeekerId =
                HttpContext.Session.GetInt32("JobSeekerId");

            if (jobSeekerId == null)
                return RedirectToAction(nameof(Login));

            var exists = await _context.JobSeekers
                .AnyAsync(j => j.JobSeekerId == jobSeekerId.Value);

            if (!exists)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        // =========================
        // BROWSE JOBS
        // =========================

        [HttpGet]
        public async Task<IActionResult> BrowseJobs()
        {
            var jobs = await _context.Jobs
                .Where(j => j.Status == "Active")
                .OrderByDescending(j => j.JobId)
                .ToListAsync();

            var employerIds = jobs
                .Select(j => j.EmployerId)
                .Distinct()
                .ToList();

            ViewBag.CompanyNames = await _context.Employers
                .Where(e => employerIds.Contains(e.EmployerId))
                .ToDictionaryAsync(
                    e => e.EmployerId,
                    e => e.CompanyName);

            return View(jobs);
        }

        // =========================
        // JOB DETAILS
        // =========================

        [HttpGet]
        public async Task<IActionResult> JobDetails(int id)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
                return NotFound();

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.EmployerId == job.EmployerId);

            ViewBag.CompanyName =
                employer?.CompanyName ?? "Company information unavailable";

            return View(job);
        }

        // =========================
        // APPLY FOR JOB - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> ApplyJob(int id)
        {
            var jobSeekerId =
                HttpContext.Session.GetInt32("JobSeekerId");

            if (jobSeekerId == null)
                return RedirectToAction(nameof(Login));

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
                return NotFound();

            if (job.Status != "Active" ||
                job.RemainingVacancies <= 0)
            {
                TempData["ErrorMessage"] =
                    "This job is closed or no vacancies remain.";

                return RedirectToAction(nameof(BrowseJobs));
            }

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a =>
                    a.JobId == job.JobId &&
                    a.JobSeekerId == jobSeekerId.Value);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] =
                    "You have already applied for this job.";

                return RedirectToAction(nameof(MyApplications));
            }

            var model = new JobApplicationViewModel
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle
            };

            return View(model);
        }

        // =========================
        // APPLY FOR JOB - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyJob(
            JobApplicationViewModel model)
        {
            var jobSeekerId =
                HttpContext.Session.GetInt32("JobSeekerId");

            if (jobSeekerId == null)
                return RedirectToAction(nameof(Login));

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j =>
                    j.JobSeekerId == jobSeekerId.Value);

            if (jobSeeker == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == model.JobId);

            if (job == null)
                return NotFound();

            if (job.Status != "Active" ||
                job.RemainingVacancies <= 0)
            {
                TempData["ErrorMessage"] =
                    "This job is closed or no vacancies remain.";

                return RedirectToAction(nameof(BrowseJobs));
            }

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a =>
                    a.JobId == job.JobId &&
                    a.JobSeekerId == jobSeeker.JobSeekerId);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] =
                    "You have already applied for this job.";

                return RedirectToAction(nameof(MyApplications));
            }

            if (!ModelState.IsValid)
            {
                model.JobTitle = job.JobTitle;
                return View(model);
            }

            var application = new JobApplication
            {
                JobId = job.JobId,
                JobSeekerId = jobSeeker.JobSeekerId,
                CoverMessage = model.CoverMessage,
                Status = "Pending",
                AppliedDate = DateTime.Now
            };

            _context.JobApplications.Add(application);

            // Applying does not decrease vacancies.
            // Vacancy counts change only after employer acceptance.
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your application has been submitted successfully.";

            return RedirectToAction(nameof(MyApplications));
        }

        // =========================
        // MY APPLICATIONS
        // =========================

        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var jobSeekerId =
                HttpContext.Session.GetInt32("JobSeekerId");

            if (jobSeekerId == null)
                return RedirectToAction(nameof(Login));

            var applications = await (
                from application in _context.JobApplications
                join job in _context.Jobs
                    on application.JobId equals job.JobId
                join employer in _context.Employers
                    on job.EmployerId equals employer.EmployerId
                where application.JobSeekerId == jobSeekerId.Value
                orderby application.AppliedDate descending
                select new
                {
                    application.ApplicationId,
                    application.JobId,
                    JobTitle = job.JobTitle,
                    CompanyName = employer.CompanyName,
                    Location = job.Location,
                    JobType = job.JobType,
                    Salary = job.Salary,
                    TotalVacancies = job.TotalVacancies,
                    FilledVacancies = job.FilledVacancies,
                    RemainingVacancies =
                        Math.Max(0, job.TotalVacancies - job.FilledVacancies),
                    CoverMessage = application.CoverMessage,
                    Status = application.Status,
                    AppliedDate = application.AppliedDate
                })
                .ToListAsync();

            return View(applications);
        }

        // =========================
        // PROFILE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var jobSeekerId =
                HttpContext.Session.GetInt32("JobSeekerId");

            if (jobSeekerId == null)
                return RedirectToAction(nameof(Login));

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j =>
                    j.JobSeekerId == jobSeekerId.Value);

            if (jobSeeker == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            var model = new JobSeekerProfileViewModel
            {
                JobSeekerId = jobSeeker.JobSeekerId,
                FullName = jobSeeker.FullName,
                Email = jobSeeker.Email,
                Phone = jobSeeker.Phone,
                Location = jobSeeker.Location,
                Skills = jobSeeker.Skills
            };

            return View(model);
        }

        // =========================
        // PROFILE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            JobSeekerProfileViewModel model)
        {
            var jobSeekerId =
                HttpContext.Session.GetInt32("JobSeekerId");

            if (jobSeekerId == null)
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(model);

            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j =>
                    j.JobSeekerId == jobSeekerId.Value);

            if (jobSeeker == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }

            var emailInUse = await _context.JobSeekers
                .AnyAsync(j =>
                    j.Email == model.Email &&
                    j.JobSeekerId != jobSeeker.JobSeekerId);

            if (emailInUse)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email address is already in use.");

                return View(model);
            }

            jobSeeker.FullName = model.FullName;
            jobSeeker.Email = model.Email;
            jobSeeker.Phone = model.Phone;
            jobSeeker.Location = model.Location;
            jobSeeker.Skills = model.Skills;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Profile updated successfully.";

            return RedirectToAction(nameof(Profile));
        }

        // =========================
        // LOGOUT
        // =========================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
