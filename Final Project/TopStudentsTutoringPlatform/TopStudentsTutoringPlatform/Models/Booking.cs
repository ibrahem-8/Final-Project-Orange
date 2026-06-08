using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopStudentsTutoringPlatform.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        public int TutorProfileId { get; set; }

        [ForeignKey("TutorProfileId")]
        public TutorProfile? TutorProfile { get; set; }

        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        public int? EducationalPackageId { get; set; }

        public EducationalPackage? EducationalPackage { get; set; }

        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = "Confirmed";

        public string PaymentStatus { get; set; } = "Pending";

        public string? MeetingLink { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Review? Review { get; set; }
        public Payment? Payment { get; set; }
    }
}