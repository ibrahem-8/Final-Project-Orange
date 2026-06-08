using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopStudentsTutoringPlatform.Data;
using TopStudentsTutoringPlatform.Models;
using TopStudentsTutoringPlatform.ViewModels;

namespace TopStudentsTutoringPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                UsersCount = await _userManager.Users.CountAsync(),
                TutorsCount = await _context.TutorProfiles.CountAsync(),
                PendingTutorsCount = await _context.TutorProfiles
                    .CountAsync(t => t.VerificationStatus == "Pending"),
                BookingsCount = await _context.Bookings.CountAsync(),
                ComplaintsCount = await _context.Complaints.CountAsync(),
                PaymentsCount = await _context.Payments.CountAsync()
            };

            return View(model);
        }
        public async Task<IActionResult> PendingTutors()
        {
            var pendingTutors = await _context.TutorProfiles
                .Include(t => t.User)
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .Where(t => t.VerificationStatus == "Pending")
                .ToListAsync();

            return View(pendingTutors);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTutor(int id)
        {
            var tutor = await _context.TutorProfiles.FindAsync(id);

            if (tutor == null)
            {
                return NotFound();
            }

            tutor.VerificationStatus = "Approved";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingTutors));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTutor(int id)
        {
            var tutor = await _context.TutorProfiles.FindAsync(id);

            if (tutor == null)
            {
                return NotFound();
            }

            tutor.VerificationStatus = "Rejected";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingTutors));
        }
        public async Task<IActionResult> PendingReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.TutorProfile)
                    .ThenInclude(t => t.User)
                .Include(r => r.Booking)
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            review.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingReviews));
        }
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Student)
                .Include(b => b.TutorProfile)
                    .ThenInclude(t => t.User)
                .Include(b => b.Subject)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }
        public async Task<IActionResult> Complaints()
        {
            var complaints = await _context.Complaints
                .Include(c => c.SubmittedBy)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.TutorProfile)
                        .ThenInclude(t => t.User)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Subject)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComplaintStatus(int id, string status)
        {
            var complaint = await _context.Complaints.FindAsync(id);

            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Complaints));
        }
        public async Task<IActionResult> Users(string? search)
        {
            var usersQuery = _userManager.Users
                .OrderBy(u => u.FullName)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                usersQuery = usersQuery.Where(u =>
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search));
            }

            var users = await usersQuery.ToListAsync();

            var usersWithRoles = new List<dynamic>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var rolesText = string.Join(", ", roles);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();

                    if (!rolesText.ToLower().Contains(searchLower) &&
                        !user.FullName.ToLower().Contains(searchLower) &&
                        !user.Email.ToLower().Contains(searchLower))
                    {
                        continue;
                    }
                }

                usersWithRoles.Add(new
                {
                    User = user,
                    Roles = rolesText,
                    IsBanned = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now
                });
            }

            ViewBag.Search = search;

            return View(usersWithRoles);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["UserError"] = "You cannot ban your own admin account.";
                return RedirectToAction(nameof(Users));
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                TempData["UserError"] = "Admin accounts cannot be banned from this page.";
                return RedirectToAction(nameof(Users));
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);

            TempData["UserSuccess"] = "User account has been banned successfully.";

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);

            TempData["UserSuccess"] = "User account has been activated successfully.";

            return RedirectToAction(nameof(Users));
        }
        public async Task<IActionResult> PendingSubjects()
        {
            var pendingSubjects = await _context.TutorSubjects
                .Include(ts => ts.Subject)
                .Include(ts => ts.TutorProfile)
                    .ThenInclude(tp => tp.User)
                .Where(ts =>
                    ts.VerificationStatus == "Pending" ||
                    ts.VerificationStatus == null ||
                    ts.VerificationStatus == "")
                .OrderByDescending(ts => ts.CreatedAt)
                .ToListAsync();

            return View(pendingSubjects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSubject(int id)
        {
            var tutorSubject = await _context.TutorSubjects
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (tutorSubject == null)
            {
                return NotFound();
            }

            tutorSubject.VerificationStatus = "Approved";

            await _context.SaveChangesAsync();

            TempData["SubjectSuccess"] = "Subject approved successfully.";

            return RedirectToAction(nameof(PendingSubjects));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSubject(int id)
        {
            var tutorSubject = await _context.TutorSubjects
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (tutorSubject == null)
            {
                return NotFound();
            }

            tutorSubject.VerificationStatus = "Rejected";

            await _context.SaveChangesAsync();

            TempData["SubjectSuccess"] = "Subject rejected successfully.";

            return RedirectToAction(nameof(PendingSubjects));
        }
    }
}