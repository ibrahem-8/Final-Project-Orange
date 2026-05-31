using System.ComponentModel.DataAnnotations;

namespace TopStudentsTutoringPlatform.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        public ICollection<TutorSubject>? TutorSubjects { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}