using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopStudentsTutoringPlatform.Models
{
    public class TutorSubject
    {
        public int Id { get; set; }

        public int TutorProfileId { get; set; }

        [ForeignKey("TutorProfileId")]
        public TutorProfile? TutorProfile { get; set; }

        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        [Required]
        [StringLength(20)]
        public string Grade { get; set; }

        public string VerificationStatus { get; set; } = "Pending";

        public string? AcademicTranscriptUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}