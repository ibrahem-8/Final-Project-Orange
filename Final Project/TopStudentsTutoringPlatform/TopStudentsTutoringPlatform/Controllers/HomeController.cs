using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TopStudentsTutoringPlatform.Models;
using Microsoft.EntityFrameworkCore;
using TopStudentsTutoringPlatform.Data;

namespace TopStudentsTutoringPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var topTutors = await _context.TutorProfiles
                .Include(t => t.User)
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .Include(t => t.Reviews)
                .Where(t => t.VerificationStatus == "Approved")
                .Select(t => new
                {
                    Tutor = t,
                    ApprovedSubjects = t.TutorSubjects
                        .Where(ts => ts.VerificationStatus == "Approved")
                        .ToList(),
                    ApprovedReviews = t.Reviews
                        .Where(r => r.IsApproved)
                        .ToList(),
                    AverageRating = t.Reviews.Any(r => r.IsApproved)
                        ? t.Reviews.Where(r => r.IsApproved).Average(r => r.Rating)
                        : 0,
                    ReviewsCount = t.Reviews.Count(r => r.IsApproved)
                })
                .OrderByDescending(t => t.AverageRating)
                .ThenByDescending(t => t.ReviewsCount)
                .Take(3)
                .ToListAsync();

            ViewBag.TopTutors = topTutors;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
