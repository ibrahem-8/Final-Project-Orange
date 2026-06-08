using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using TopStudentsTutoringPlatform.Data;
using TopStudentsTutoringPlatform.Models;

namespace TopStudentsTutoringPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public PaymentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Checkout(int bookingId)
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
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.StudentId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Payment == null)
            {
                return BadRequest("Payment record was not found for this booking.");
            }

            if (booking.Payment.PaymentStatus == "Paid")
            {
                return RedirectToAction("MyBookings", "Student");
            }

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(booking.Payment.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Tutoring Session - {booking.Subject?.Name}",
                                Description = $"Tutor: {booking.TutorProfile?.User?.FullName}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = domain + $"/Payments/Success?bookingId={booking.Id}",
                CancelUrl = domain + "/Student/MyBookings"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.StudentId == user.Id);

            if (booking == null || booking.Payment == null)
            {
                return NotFound();
            }

            booking.PaymentStatus = "Paid";
            booking.Payment.PaymentStatus = "Paid";
            await _context.SaveChangesAsync();

            TempData["PaymentSuccess"] = "Payment completed successfully.";

            return RedirectToAction("MyBookings", "Student");
        }
    }
}