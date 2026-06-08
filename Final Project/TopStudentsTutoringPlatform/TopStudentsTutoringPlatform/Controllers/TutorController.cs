using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopStudentsTutoringPlatform.Data;
using TopStudentsTutoringPlatform.Models;

namespace TopStudentsTutoringPlatform.Controllers
{
    [Authorize(Roles = "Tutor")]
    public class TutorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TutorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            ViewBag.TutorProfile = tutorProfile;

            return View();
        }

        public async Task<IActionResult> CreateProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var existingProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(
            TutorProfile tutorProfile,
            IFormFile academicTranscriptFile,
            IFormFile? cvFile,
            string initialSubjectName,
            string initialGrade)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            tutorProfile.UserId = user.Id;
            tutorProfile.VerificationStatus = "Pending";
            tutorProfile.CreatedAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(initialSubjectName))
            {
                ModelState.AddModelError("initialSubjectName", "You must add at least one subject.");
            }

            if (string.IsNullOrWhiteSpace(initialGrade))
            {
                ModelState.AddModelError("initialGrade", "You must add the grade for the subject.");
            }

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Bookings");
            ModelState.Remove("Reviews");
            ModelState.Remove("TutorSubjects");
            ModelState.Remove("EducationalPackages");
            ModelState.Remove("TutorAvailabilities");
            ModelState.Remove("Payments");
            ModelState.Remove("AcademicTranscriptUrl");
            ModelState.Remove("CVUrl");

            if (academicTranscriptFile == null || academicTranscriptFile.Length == 0)
            {
                ModelState.AddModelError("AcademicTranscriptUrl", "Academic transcript is required.");
            }

            if (ModelState.IsValid)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "tutor-documents"
                );

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (academicTranscriptFile != null && academicTranscriptFile.Length > 0)
                {
                    var transcriptFileName = Guid.NewGuid().ToString() + Path.GetExtension(academicTranscriptFile.FileName);
                    var transcriptFilePath = Path.Combine(uploadsFolder, transcriptFileName);

                    using (var stream = new FileStream(transcriptFilePath, FileMode.Create))
                    {
                        await academicTranscriptFile.CopyToAsync(stream);
                    }

                    tutorProfile.AcademicTranscriptUrl = "/uploads/tutor-documents/" + transcriptFileName;
                }

                if (cvFile != null && cvFile.Length > 0)
                {
                    var cvFileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName);
                    var cvFilePath = Path.Combine(uploadsFolder, cvFileName);

                    using (var stream = new FileStream(cvFilePath, FileMode.Create))
                    {
                        await cvFile.CopyToAsync(stream);
                    }

                    tutorProfile.CVUrl = "/uploads/tutor-documents/" + cvFileName;
                }

                _context.TutorProfiles.Add(tutorProfile);
                await _context.SaveChangesAsync();

                var subjectName = initialSubjectName.Trim();

                var subject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == subjectName.ToLower());

                if (subject == null)
                {
                    subject = new Subject
                    {
                        Name = subjectName
                    };

                    _context.Subjects.Add(subject);
                    await _context.SaveChangesAsync();
                }

                var tutorSubject = new TutorSubject
                {
                    TutorProfileId = tutorProfile.Id,
                    SubjectId = subject.Id,
                    Grade = initialGrade.Trim(),
                    VerificationStatus = "Approved",
                    AcademicTranscriptUrl = tutorProfile.AcademicTranscriptUrl,
                    CreatedAt = DateTime.Now
                };

                _context.TutorSubjects.Add(tutorSubject);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Dashboard));
            }

            return View(tutorProfile);
        }
        public async Task<IActionResult> ManageSubjects()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            return View(tutorProfile);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubject(string subjectName, string grade, IFormFile subjectTranscriptFile)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            if (string.IsNullOrWhiteSpace(subjectName) || string.IsNullOrWhiteSpace(grade))
            {
                TempData["SubjectError"] = "Subject name and grade are required.";
                return RedirectToAction(nameof(ManageSubjects));
            }

            if (subjectTranscriptFile == null || subjectTranscriptFile.Length == 0)
            {
                TempData["SubjectError"] = "You must upload a transcript or proof for the new subject.";
                return RedirectToAction(nameof(ManageSubjects));
            }

            subjectName = subjectName.Trim();
            grade = grade.Trim();

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(subjectTranscriptFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["SubjectError"] = "Only PDF, JPG, JPEG, and PNG files are allowed.";
                return RedirectToAction(nameof(ManageSubjects));
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.ToLower() == subjectName.ToLower());

            if (subject == null)
            {
                subject = new Subject
                {
                    Name = subjectName
                };

                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
            }

            var alreadyExists = await _context.TutorSubjects
                .AnyAsync(ts => ts.TutorProfileId == tutorProfile.Id && ts.SubjectId == subject.Id);

            if (alreadyExists)
            {
                TempData["SubjectError"] = "This subject already exists in your profile.";
                return RedirectToAction(nameof(ManageSubjects));
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "subject-transcripts"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await subjectTranscriptFile.CopyToAsync(stream);
            }

            var tutorSubject = new TutorSubject
            {
                TutorProfileId = tutorProfile.Id,
                SubjectId = subject.Id,
                Grade = grade,
                VerificationStatus = "Pending",
                AcademicTranscriptUrl = "/uploads/subject-transcripts/" + fileName,
                CreatedAt = DateTime.Now
            };

            _context.TutorSubjects.Add(tutorSubject);
            await _context.SaveChangesAsync();

            TempData["SubjectSuccess"] = "Subject submitted successfully and is waiting for admin approval.";

            return RedirectToAction(nameof(ManageSubjects));
        }
        public async Task<IActionResult> ManagePackages()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .Include(t => t.TutorSubjects)
                    .ThenInclude(ts => ts.Subject)
                .Include(t => t.EducationalPackages)
                    .ThenInclude(p => p.Subject)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            ViewBag.ApprovedSubjects = tutorProfile.TutorSubjects?
                .Where(ts => ts.VerificationStatus == "Approved")
                .ToList();

            return View(tutorProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPackage(
            int subjectId,
            string title,
            string description,
            int numberOfHours,
            decimal price)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var tutorHasSubject = await _context.TutorSubjects.AnyAsync(ts =>
                ts.TutorProfileId == tutorProfile.Id &&
                ts.SubjectId == subjectId &&
                ts.VerificationStatus == "Approved");

            if (!tutorHasSubject)
            {
                TempData["PackageError"] = "You can create packages only for approved subjects.";
                return RedirectToAction(nameof(ManagePackages));
            }

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description) ||
                numberOfHours <= 0 ||
                price <= 0)
            {
                TempData["PackageError"] = "Please fill all package fields correctly.";
                return RedirectToAction(nameof(ManagePackages));
            }

            var normalPrice = tutorProfile.HourlyRate * numberOfHours;

            if (price >= normalPrice)
            {
                TempData["PackageError"] = $"Package price must be less than the normal price ({normalPrice:0.00} JOD).";
                return RedirectToAction(nameof(ManagePackages));
            }

            var package = new EducationalPackage
            {
                TutorProfileId = tutorProfile.Id,
                SubjectId = subjectId,
                Title = title.Trim(),
                Description = description.Trim(),
                NumberOfHours = numberOfHours,
                Price = price,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.EducationalPackages.Add(package);
            await _context.SaveChangesAsync();

            TempData["PackageSuccess"] = "Educational package added successfully.";

            return RedirectToAction(nameof(ManagePackages));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePackageStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var package = await _context.EducationalPackages
                .FirstOrDefaultAsync(p => p.Id == id && p.TutorProfileId == tutorProfile.Id);

            if (package == null)
            {
                return NotFound();
            }

            package.IsActive = !package.IsActive;

            await _context.SaveChangesAsync();

            TempData["PackageSuccess"] = package.IsActive
                ? "Package activated successfully."
                : "Package deactivated successfully.";

            return RedirectToAction(nameof(ManagePackages));
        }
        public async Task<IActionResult> ManageAvailability()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .Include(t => t.TutorAvailabilities)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            return View(tutorProfile);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(string dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            if (string.IsNullOrWhiteSpace(dayOfWeek) || startTime >= endTime)
            {
                return RedirectToAction(nameof(ManageAvailability));
            }

            var availability = new TutorAvailability
            {
                TutorProfileId = tutorProfile.Id,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.TutorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageAvailability));
        }
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var bookings = await _context.Bookings
                .Include(b => b.Student)
                .Include(b => b.Subject)
                .Include(b => b.Payment)
                .Where(b => b.TutorProfileId == tutorProfile.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }
        public async Task<IActionResult> SubmitComplaint(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var booking = await _context.Bookings
    .Include(b => b.Student)
    .Include(b => b.Subject)
    .FirstOrDefaultAsync(b => b.Id == bookingId && b.TutorProfileId == tutorProfile.Id);

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

            var tutorProfile = await _context.TutorProfiles
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tutorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == complaint.BookingId && b.TutorProfileId == tutorProfile.Id);

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
    .Include(b => b.Student)
    .Include(b => b.Subject)
    .FirstOrDefaultAsync(b => b.Id == booking.Id);

            ViewBag.Booking = bookingWithDetails;
            return View(complaint);
        }
    }
}