using LocalJobPortal.Data;
using LocalJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalJobPortal.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ADMIN LOGIN
        // =========================================================

        // GET: /Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a =>
                    a.Username == username &&
                    a.Password == password);

            if (admin == null)
            {
                ViewBag.ErrorMessage =
                    "Invalid username or password.";

                return View();
            }

            HttpContext.Session.SetInt32(
                "AdminId",
                admin.AdminId);

            return RedirectToAction("Dashboard");
        }

        // =========================================================
        // ADMIN DASHBOARD
        // =========================================================

        // GET: /Admin/Dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            var adminId =
                HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // =========================================================
        // VIEW EMPLOYERS
        // =========================================================

        // GET: /Admin/ViewEmployers
        [HttpGet]
        public async Task<IActionResult> ViewEmployers()
        {
            var adminId =
                HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            var employers = await _context.Employers
                .OrderByDescending(e => e.EmployerId)
                .ToListAsync();

            return View(employers);
        }

        // =========================================================
        // VIEW JOB SEEKERS
        // =========================================================

        // GET: /Admin/ViewJobSeekers
        [HttpGet]
        public async Task<IActionResult> ViewJobSeekers()
        {
            var adminId =
                HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            var jobSeekers = await _context.JobSeekers
                .OrderByDescending(j => j.JobSeekerId)
                .ToListAsync();

            return View(jobSeekers);
        }

        // =========================================================
        // MANAGE JOBS
        // =========================================================

        // GET: /Admin/ManageJobs
        [HttpGet]
        public async Task<IActionResult> ManageJobs()
        {
            var adminId =
                HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            var jobs = await _context.Jobs
                .Join(
                    _context.Employers,
                    job => job.EmployerId,
                    employer => employer.EmployerId,
                    (job, employer) => new ManageJobViewModel
                    {
                        JobId = job.JobId,
                        JobTitle = job.JobTitle,
                        Location = job.Location,
                        JobType = job.JobType,
                        Salary = job.Salary,
                        RequiredSkills = job.RequiredSkills,
                        Status = job.Status
                    })
                .OrderByDescending(j => j.JobId)
                .ToListAsync();

            return View(jobs);
        }

        // =========================================================
        // ADMIN CLOSE JOB
        // =========================================================

        // GET: /Admin/CloseJob/5
        [HttpGet]
        public async Task<IActionResult> CloseJob(int id)
        {
            var adminId =
                HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
            {
                return NotFound();
            }

            job.Status = "Closed";

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageJobs");
        }

        // =========================================================
        // ADMIN DELETE JOB
        // =========================================================

        // GET: /Admin/DeleteJob/5
        [HttpGet]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var adminId =
                HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageJobs");
        }

        // =========================================================
        // VIEW APPLICATIONS
        // =========================================================

        // GET: /Admin/ViewApplications
        [HttpGet]
        public async Task<IActionResult> ViewApplications()
        {
            var adminId =
                HttpContext.Session.GetInt32("AdminId");

            if (adminId == null)
            {
                return RedirectToAction("Login");
            }

            var applications = await _context.JobApplications
                .Join(
                    _context.Jobs,
                    application => application.JobId,
                    job => job.JobId,
                    (application, job) => new
                    {
                        application,
                        job
                    })
                .Join(
                    _context.JobSeekers,
                    x => x.application.JobSeekerId,
                    seeker => seeker.JobSeekerId,
                    (x, seeker) => new ApplicantViewModel
                    {
                        ApplicationId =
                            x.application.ApplicationId,

                        JobId =
                            x.job.JobId,

                        JobTitle =
                            x.job.JobTitle,

                        FullName =
                            seeker.FullName,

                        Email =
                            seeker.Email,

                        Phone =
                            seeker.Phone,

                        CoverMessage =
                            x.application.CoverMessage,

                        Status =
                            x.application.Status,

                        AppliedDate =
                            x.application.AppliedDate
                    })
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            return View(applications);
        }

        // =========================================================
        // ADMIN LOGOUT
        // =========================================================

        // GET: /Admin/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminId");

            return RedirectToAction("Login");
        }
    }
}