using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopStudentsTutoringPlatform.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        public int TutorProfileId { get; set; }

        [ForeignKey("TutorProfileId")]
        public TutorProfile? TutorProfile { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlatformCommission { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TutorEarning { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Mock Payment";

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}