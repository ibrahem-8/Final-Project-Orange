using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopStudentsTutoringPlatform.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        public int TutorProfileId { get; set; }

        [ForeignKey("TutorProfileId")]
        public TutorProfile? TutorProfile { get; set; }

        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}