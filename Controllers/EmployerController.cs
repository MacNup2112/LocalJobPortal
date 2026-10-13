using LocalJobPortal.Data;
using LocalJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalJobPortal.Controllers
{
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Employer/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Employer/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(EmployerRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingEmployer = await _context.Employers
                .FirstOrDefaultAsync(e => e.Email == model.Email);

            if (existingEmployer != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An employer with this email already exists.");

                return View(model);
            }

            var employer = new Employer
            {
                CompanyName = model.CompanyName,
                ContactPerson = model.ContactPerson,
                Email = model.Email,
                Phone = model.Phone,
                Location = model.Location,
                Password = model.Password
            };

            _context.Employers.Add(employer);
            await _context.SaveChangesAsync();

            return RedirectToAction("RegisterSuccess");
        }

        // GET: /Employer/RegisterSuccess
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        // GET: /Employer/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Employer/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(EmployerLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e =>
                    e.Email == model.Email &&
                    e.Password == model.Password);

            if (employer == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32(
                "EmployerId",
                employer.EmployerId);

            return RedirectToAction("Dashboard");
        }

        // GET: /Employer/Dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            return View();
        }

        // GET: /Employer/PostJob
        [HttpGet]
        public IActionResult PostJob()
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            return View(new JobPostViewModel());
        }

        // POST: /Employer/PostJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(JobPostViewModel model)
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var employerExists = await _context.Employers
                .AnyAsync(e => e.EmployerId == employerId.Value);

            if (!employerExists)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            var job = new Job
            {
                JobTitle = model.JobTitle,
                Location = model.Location,
                JobType = model.JobType,
                Salary = model.Salary,
                RequiredSkills = model.RequiredSkills,
                JobDescription = model.JobDescription,
                TotalVacancies = model.TotalVacancies,
                FilledVacancies = 0,
                EmployerId = employerId.Value,
                Status = "Active"
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job posted successfully.";

            return RedirectToAction("ManageJobs");
        }

        // GET: /Employer/ManageJobs
        [HttpGet]
        public async Task<IActionResult> ManageJobs()
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var jobs = await _context.Jobs
                .Where(j => j.EmployerId == employerId.Value)
                .Select(j => new ManageJobViewModel
                {
                    JobId = j.JobId,
                    JobTitle = j.JobTitle,
                    Location = j.Location,
                    JobType = j.JobType,
                    Salary = j.Salary,
                    RequiredSkills = j.RequiredSkills,
                    Status = j.Status,

                    // Vacancy information
                    TotalVacancies = j.TotalVacancies,
                    FilledVacancies = j.FilledVacancies

                    // RemainingVacancies is calculated
                    // automatically in ManageJobViewModel.
                })
                .ToListAsync();

            return View(jobs);
        }

        // GET: /Employer/DeleteJob/5
        [HttpGet]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employerId.Value);

            if (job == null)
                return NotFound();

            var hasApplications = await _context.JobApplications
                .AnyAsync(a => a.JobId == id);

            if (hasApplications)
            {
                TempData["ErrorMessage"] =
                    "This job has applications. Close it instead of deleting it.";

                return RedirectToAction("ManageJobs");
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job deleted successfully.";

            return RedirectToAction("ManageJobs");
        }

        // GET: /Employer/CloseJob/5
        [HttpGet]
        public async Task<IActionResult> CloseJob(int id)
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employerId.Value);

            if (job == null)
                return NotFound();

            job.Status = "Closed";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job closed successfully.";

            return RedirectToAction("ManageJobs");
        }

        // GET: /Employer/EditJob/5
        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employerId.Value);

            if (job == null)
                return NotFound();

            var model = new EditJobViewModel
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                Location = job.Location,
                JobType = job.JobType,
                Salary = job.Salary,
                RequiredSkills = job.RequiredSkills,
                JobDescription = job.JobDescription
            };

            return View(model);
        }

        // POST: /Employer/EditJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(EditJobViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == model.JobId &&
                    j.EmployerId == employerId.Value);

            if (job == null)
                return NotFound();

            job.JobTitle = model.JobTitle;
            job.Location = model.Location;
            job.JobType = model.JobType;
            job.Salary = model.Salary;
            job.RequiredSkills = model.RequiredSkills;
            job.JobDescription = model.JobDescription;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job updated successfully.";

            return RedirectToAction("ManageJobs");
        }

        // GET: /Employer/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.EmployerId == employerId.Value);

            if (employer == null)
                return RedirectToAction("Login");

            var model = new EmployerProfileViewModel
            {
                EmployerId = employer.EmployerId,
                CompanyName = employer.CompanyName,
                ContactPerson = employer.ContactPerson,
                Email = employer.Email,
                Phone = employer.Phone,
                Location = employer.Location
            };

            return View(model);
        }

        // POST: /Employer/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(EmployerProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.EmployerId == employerId.Value);

            if (employer == null)
                return RedirectToAction("Login");

            employer.CompanyName = model.CompanyName;
            employer.ContactPerson = model.ContactPerson;
            employer.Email = model.Email;
            employer.Phone = model.Phone;
            employer.Location = model.Location;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Company profile updated successfully.";

            return RedirectToAction("Profile");
        }

        // GET: /Employer/ViewApplicants
        [HttpGet]
        public async Task<IActionResult> ViewApplicants()
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var applicants = await _context.JobApplications
                .Join(
                    _context.Jobs,
                    application => application.JobId,
                    job => job.JobId,
                    (application, job) => new { application, job })
                .Where(x => x.job.EmployerId == employerId.Value)
                .Join(
                    _context.JobSeekers,
                    x => x.application.JobSeekerId,
                    seeker => seeker.JobSeekerId,
                    (x, seeker) => new ApplicantViewModel
                    {
                        ApplicationId = x.application.ApplicationId,
                        JobId = x.job.JobId,
                        JobTitle = x.job.JobTitle,
                        FullName = seeker.FullName,
                        Email = seeker.Email,
                        Phone = seeker.Phone,
                        CoverMessage = x.application.CoverMessage,
                        Status = x.application.Status,
                        AppliedDate = x.application.AppliedDate
                    })
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            return View(applicants);
        }

        // GET: /Employer/AcceptApplication/5
        [HttpGet]
        public async Task<IActionResult> AcceptApplication(int id)
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var application = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == application.JobId &&
                    j.EmployerId == employerId.Value);

            if (job == null)
                return NotFound();

            if (application.Status == "Accepted")
            {
                TempData["ErrorMessage"] =
                    "This application has already been accepted.";

                return RedirectToAction("ViewApplicants");
            }

            if (application.Status == "Rejected")
            {
                TempData["ErrorMessage"] =
                    "A rejected application cannot be accepted.";

                return RedirectToAction("ViewApplicants");
            }

            if (job.Status != "Active" ||
                job.FilledVacancies >= job.TotalVacancies)
            {
                job.Status = "Closed";

                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] =
                    "No vacancies are available for this job.";

                return RedirectToAction("ViewApplicants");
            }

            application.Status = "Accepted";
            job.FilledVacancies++;

            if (job.FilledVacancies >= job.TotalVacancies)
            {
                job.Status = "Closed";
            }

            await _context.SaveChangesAsync();

            int remaining = Math.Max(
                0,
                job.TotalVacancies - job.FilledVacancies);

            TempData["SuccessMessage"] =
                $"Application accepted successfully. Remaining vacancies: {remaining}.";

            return RedirectToAction("ViewApplicants");
        }

        // GET: /Employer/RejectApplication/5
        [HttpGet]
        public async Task<IActionResult> RejectApplication(int id)
        {
            var employerId = HttpContext.Session.GetInt32("EmployerId");

            if (employerId == null)
                return RedirectToAction("Login");

            var application = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == application.JobId &&
                    j.EmployerId == employerId.Value);

            if (job == null)
                return NotFound();

            if (application.Status == "Accepted")
            {
                TempData["ErrorMessage"] =
                    "An accepted application cannot be rejected directly.";

                return RedirectToAction("ViewApplicants");
            }

            if (application.Status == "Rejected")
            {
                TempData["ErrorMessage"] =
                    "This application has already been rejected.";

                return RedirectToAction("ViewApplicants");
            }

            application.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Application rejected successfully.";

            return RedirectToAction("ViewApplicants");
        }

        // GET: /Employer/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}