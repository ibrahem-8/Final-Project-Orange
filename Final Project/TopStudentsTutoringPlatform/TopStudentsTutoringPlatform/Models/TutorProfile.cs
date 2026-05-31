using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopStudentsTutoringPlatform.Models
{
    public class TutorProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(100)]
        public string University { get; set; }

        [Required]
        [StringLength(100)]
        public string Major { get; set; }

        [Range(0, 4)]
        public double GPA { get; set; }

        [Required]
        [StringLength(500)]
        public string Bio { get; set; }

        [Required]
        [StringLength(300)]
        public string Subjects { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public string VerificationStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<TutorSubject>? TutorSubjects { get; set; }
        public ICollection<EducationalPackage>? EducationalPackages { get; set; }
        public ICollection<TutorAvailability>? TutorAvailabilities { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}