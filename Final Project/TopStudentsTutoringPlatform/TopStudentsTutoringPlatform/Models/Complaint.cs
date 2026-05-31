using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopStudentsTutoringPlatform.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        public string SubmittedById { get; set; }

        [ForeignKey("SubmittedById")]
        public ApplicationUser? SubmittedBy { get; set; }

        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(800)]
        public string Description { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}