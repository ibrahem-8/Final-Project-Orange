using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopStudentsTutoringPlatform.Data;
using TopStudentsTutoringPlatform.Models;

namespace TopStudentsTutoringPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard(string? search)
        {
            var tutorsQuery = _context.TutorProfiles
                .Include(t => t.User)
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .Where(t => t.VerificationStatus == "Approved")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                tutorsQuery = tutorsQuery.Where(t =>
                    t.User.FullName.ToLower().Contains(search) ||
                    t.University.ToLower().Contains(search) ||
                    t.Major.ToLower().Contains(search) ||
                    t.TutorSubjects.Any(ts =>
                        ts.VerificationStatus == "Approved" &&
                        ts.Subject.Name.ToLower().Contains(search))
                );
            }

            ViewBag.Search = search;

            var tutors = await tutorsQuery.ToListAsync();

            return View(tutors);
        }
        public async Task<IActionResult> TutorDetails(int id)
        {
            var tutor = await _context.TutorProfiles
                .Include(t => t.User)
                .Include(t => t.EducationalPackages)
                    .ThenInclude(p => p.Subject)
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .Include(t => t.TutorAvailabilities)
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.Student)
                .FirstOrDefaultAsync(t => t.Id == id && t.VerificationStatus == "Approved");

            if (tutor == null)
            {
                return NotFound();
            }

            tutor.Reviews = tutor.Reviews?
                .Where(r => r.IsApproved)
                .ToList();

            tutor.TutorSubjects = tutor.TutorSubjects?
                .Where(ts => ts.VerificationStatus == "Approved")
                .ToList();

            tutor.EducationalPackages = tutor.EducationalPackages?
                .Where(p => p.IsActive)
                .ToList();

            return View(tutor);
        }
        public async Task<IActionResult> BookSession(int id)
        {
            var tutor = await _context.TutorProfiles
                .Include(t => t.User)
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .Include(t => t.TutorAvailabilities)
                .FirstOrDefaultAsync(t => t.Id == id && t.VerificationStatus == "Approved");

            if (tutor == null)
            {
                return NotFound();
            }

            ViewBag.Tutor = tutor;
            ViewBag.TutorSubjects = tutor.TutorSubjects?
                .Where(ts => ts.VerificationStatus == "Approved")
                .ToList();
            ViewBag.TutorAvailabilities = tutor.TutorAvailabilities?
                .Where(a => a.IsActive)
                .ToList();

            var booking = new Booking
            {
                TutorProfileId = tutor.Id,
                BookingDate = DateTime.Today
            };

            return View(booking);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookSession(Booking booking, int availabilityId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutor = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.Id == booking.TutorProfileId && t.VerificationStatus == "Approved");

            if (tutor == null)
            {
                return NotFound();
            }
            booking.Id = 0;
            var availability = await _context.TutorAvailabilities
    .FirstOrDefaultAsync(a =>
        a.Id == availabilityId &&
        a.TutorProfileId == tutor.Id &&
        a.IsActive);

            if (availability == null)
            {
                ModelState.AddModelError("", "Please select a valid available time.");
            }
            else
            {
                var isAlreadyBooked = await _context.Bookings.AnyAsync(b =>
                    b.TutorProfileId == booking.TutorProfileId &&
                    b.BookingDate.Date == booking.BookingDate.Date &&
                    b.StartTime == availability.StartTime &&
                    b.EndTime == availability.EndTime &&
                    b.Status != "Cancelled");

                if (isAlreadyBooked)
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please choose another available time.");
                }

                booking.StartTime = availability.StartTime;
                booking.EndTime = availability.EndTime;

                var selectedDayName = booking.BookingDate.DayOfWeek.ToString();

                if (selectedDayName != availability.DayOfWeek)
                {
                    ModelState.AddModelError("BookingDate", $"Selected date must be a {availability.DayOfWeek}.");
                }
            }
            var tutorHasSubject = await _context.TutorSubjects
                .AnyAsync(ts =>
                    ts.TutorProfileId == tutor.Id &&
                    ts.SubjectId == booking.SubjectId &&
                    ts.VerificationStatus == "Approved");

            if (!tutorHasSubject)
            {
                ModelState.AddModelError("SubjectId", "Please select a valid subject for this tutor.");
            }

            booking.StudentId = user.Id;
            booking.Status = "Confirmed";
            booking.PaymentStatus = "Pending";
            booking.CreatedAt = DateTime.Now;

            booking.MeetingLink = $"https://meet.jit.si/topstudents-booking-{Guid.NewGuid()}";

            ModelState.Remove("Id");
            ModelState.Remove("StudentId");
            ModelState.Remove("Student");
            ModelState.Remove("TutorProfile");
            ModelState.Remove("Subject");
            ModelState.Remove("Payment");
            ModelState.Remove("Review");
            ModelState.Remove("MeetingLink");
            ModelState.Remove("Status");
            ModelState.Remove("PaymentStatus");

            if (ModelState.IsValid)
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                var amount = tutor.HourlyRate;
                var commission = amount * 0.10m;
                var tutorEarning = amount - commission;

                var payment = new Payment
                {
                    BookingId = booking.Id,
                    StudentId = user.Id,
                    TutorProfileId = tutor.Id,
                    Amount = amount,
                    PlatformCommission = commission,
                    TutorEarning = tutorEarning,
                    PaymentMethod = "Stripe",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyBookings));
            }

            

            var tutorWithDetails = await _context.TutorProfiles
    .Include(t => t.User)
    .Include(t => t.TutorSubjects)
        .ThenInclude(ts => ts.Subject)
    .Include(t => t.TutorAvailabilities)
    .FirstOrDefaultAsync(t => t.Id == tutor.Id);

            ViewBag.Tutor = tutorWithDetails;
            ViewBag.TutorSubjects = tutorWithDetails?.TutorSubjects?
                .Where(ts => ts.VerificationStatus == "Approved")
                .ToList();
            ViewBag.TutorAvailabilities = tutorWithDetails?.TutorAvailabilities?
                .Where(a => a.IsActive)
                .ToList();

            return View(booking);
        }

        public async Task<IActionResult> BookPackage(int packageId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var package = await _context.EducationalPackages
    .Include(p => p.TutorProfile)
        .ThenInclude(t => t.User)
    .Include(p => p.TutorProfile)
        .ThenInclude(t => t.TutorAvailabilities)
    .Include(p => p.Subject)
    .FirstOrDefaultAsync(p => p.Id == packageId && p.IsActive);

            if (package == null)
            {
                return NotFound();
            }

            var booking = new Booking
            {
                TutorProfileId = package.TutorProfileId,
                SubjectId = package.SubjectId,
                EducationalPackageId = package.Id,
                BookingDate = DateTime.Today
            };

            ViewBag.Package = package;

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookPackage(Booking booking, int availabilityId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var package = await _context.EducationalPackages
                .Include(p => p.TutorProfile)
                .Include(p => p.Subject)
                .FirstOrDefaultAsync(p => p.Id == booking.EducationalPackageId && p.IsActive);

            if (package == null)
            {
                return NotFound();
            }

            var availability = await _context.TutorAvailabilities
                .FirstOrDefaultAsync(a =>
                    a.Id == availabilityId &&
                    a.TutorProfileId == package.TutorProfileId &&
                    a.IsActive);

            if (availability == null)
            {
                ModelState.AddModelError("", "Please select a valid available time.");
            }
            else
            {
                var isAlreadyBooked = await _context.Bookings.AnyAsync(b =>
                    b.TutorProfileId == package.TutorProfileId &&
                    b.BookingDate.Date == booking.BookingDate.Date &&
                    b.StartTime == availability.StartTime &&
                    b.EndTime == availability.EndTime &&
                    b.Status != "Cancelled");

                if (isAlreadyBooked)
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please choose another available time.");
                }

                booking.StartTime = availability.StartTime;
                booking.EndTime = availability.EndTime;

                var selectedDayName = booking.BookingDate.DayOfWeek.ToString();

                if (selectedDayName != availability.DayOfWeek)
                {
                    ModelState.AddModelError("BookingDate", $"Selected date must be a {availability.DayOfWeek}.");
                }
            }

            booking.Id = 0;
            booking.StudentId = user.Id;
            booking.TutorProfileId = package.TutorProfileId;
            booking.SubjectId = package.SubjectId;
            booking.EducationalPackageId = package.Id;
            booking.Status = "Confirmed";
            booking.PaymentStatus = "Pending";
            booking.CreatedAt = DateTime.Now;
            booking.MeetingLink = $"https://meet.jit.si/topstudents-package-{Guid.NewGuid()}";

            ModelState.Remove("Id");
            ModelState.Remove("StudentId");
            ModelState.Remove("Student");
            ModelState.Remove("TutorProfile");
            ModelState.Remove("Subject");
            ModelState.Remove("Payment");
            ModelState.Remove("Review");
            ModelState.Remove("MeetingLink");
            ModelState.Remove("Status");
            ModelState.Remove("PaymentStatus");
            ModelState.Remove("EducationalPackage");

            if (ModelState.IsValid)
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                var amount = package.Price;
                var commission = amount * 0.10m;
                var tutorEarning = amount - commission;

                var payment = new Payment
                {
                    BookingId = booking.Id,
                    StudentId = user.Id,
                    TutorProfileId = package.TutorProfileId,
                    Amount = amount,
                    PlatformCommission = commission,
                    TutorEarning = tutorEarning,
                    PaymentMethod = "Stripe",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyBookings));
            }

            var packageWithDetails = await _context.EducationalPackages
    .Include(p => p.TutorProfile)
        .ThenInclude(t => t.User)
    .Include(p => p.TutorProfile)
        .ThenInclude(t => t.TutorAvailabilities)
    .Include(p => p.Subject)
    .FirstOrDefaultAsync(p => p.Id == package.Id);

            ViewBag.Package = packageWithDetails;
            return View(booking);
        }
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var bookings = await _context.Bookings
                .Include(b => b.TutorProfile)
                    .ThenInclude(t => t.User)
                .Include(b => b.Subject)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .Where(b => b.StudentId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }
        public async Task<IActionResult> AddReview(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Bookings
                .Include(b => b.TutorProfile)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.StudentId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            var sessionEndDateTime = booking.BookingDate.Date.Add(booking.EndTime);

            if (DateTime.Now < sessionEndDateTime)
            {
                TempData["ReviewError"] = "You can review this tutor only after the session has ended.";
                return RedirectToAction(nameof(MyBookings));
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == booking.Id);

            if (existingReview != null)
            {
                TempData["ReviewError"] = "You have already submitted a review for this booking.";
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.Booking = booking;

            var review = new Review
            {
                BookingId = booking.Id,
                TutorProfileId = booking.TutorProfileId
            };

            return View(review);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Review review)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == review.BookingId && b.StudentId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            var sessionEndDateTime = booking.BookingDate.Date.Add(booking.EndTime);

            if (DateTime.Now < sessionEndDateTime)
            {
                TempData["ReviewError"] = "You can review this tutor only after the session has ended.";
                return RedirectToAction(nameof(MyBookings));
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == booking.Id);

            if (existingReview != null)
            {
                TempData["ReviewError"] = "You have already submitted a review for this booking.";
                return RedirectToAction(nameof(MyBookings));
            }

            review.StudentId = user.Id;
            review.TutorProfileId = booking.TutorProfileId;
            review.BookingId = booking.Id;
            review.IsApproved = false;
            review.CreatedAt = DateTime.Now;

            ModelState.Remove("StudentId");
            ModelState.Remove("Student");
            ModelState.Remove("TutorProfile");
            ModelState.Remove("Booking");
            ModelState.Remove("IsApproved");

            if (ModelState.IsValid)
            {
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["ReviewSuccess"] = "Your review has been submitted and is waiting for admin approval.";
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.Booking = booking;
            return View(review);
        }
        public async Task<IActionResult> SubmitComplaint(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Bookings
    .Include(b => b.TutorProfile)
        .ThenInclude(t => t.User)
    .Include(b => b.Subject)
    .FirstOrDefaultAsync(b => b.Id == bookingId && b.StudentId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            var sessionEndDateTime = booking.BookingDate.Date.Add(booking.EndTime);

            if (DateTime.Now < sessionEndDateTime)
            {
                TempData["ComplaintError"] = "You can submit a complaint only after the session has ended.";
                return RedirectToAction(nameof(MyBookings));
            }

            var existingComplaint = await _context.Complaints
    .FirstOrDefaultAsync(c => c.BookingId == booking.Id && c.SubmittedById == user.Id);

            if (existingComplaint != null)
            {
                TempData["ComplaintError"] = "You have already submitted a complaint for this booking.";
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.Booking = booking;

            var complaint = new Complaint
            {
                BookingId = booking.Id
            };

            return View(complaint);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComplaint(Complaint complaint)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == complaint.BookingId && b.StudentId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            var sessionEndDateTime = booking.BookingDate.Date.Add(booking.EndTime);

            if (DateTime.Now < sessionEndDateTime)
            {
                TempData["ComplaintError"] = "You can submit a complaint only after the session has ended.";
                return RedirectToAction(nameof(MyBookings));
            }

            var existingComplaint = await _context.Complaints
    .FirstOrDefaultAsync(c => c.BookingId == booking.Id && c.SubmittedById == user.Id);

            if (existingComplaint != null)
            {
                TempData["ComplaintError"] = "You have already submitted a complaint for this booking.";
                return RedirectToAction(nameof(MyBookings));
            }

            complaint.SubmittedById = user.Id;
            complaint.BookingId = booking.Id;
            complaint.Status = "Pending";
            complaint.CreatedAt = DateTime.Now;

            ModelState.Remove("SubmittedById");
            ModelState.Remove("SubmittedBy");
            ModelState.Remove("Booking");

            if (ModelState.IsValid)
            {
                _context.Complaints.Add(complaint);
                await _context.SaveChangesAsync();

                TempData["ComplaintSuccess"] = "Your complaint has been submitted successfully.";
                return RedirectToAction(nameof(MyBookings));
            }

            var bookingWithDetails = await _context.Bookings
    .Include(b => b.TutorProfile)
        .ThenInclude(t => t.User)
    .Include(b => b.Subject)
    .FirstOrDefaultAsync(b => b.Id == booking.Id);

            ViewBag.Booking = bookingWithDetails;
            return View(complaint);
        }
    }
}