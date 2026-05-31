using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopStudentsTutoringPlatform.Models
{
    public class TutorAvailability
    {
        public int Id { get; set; }

        public int TutorProfileId { get; set; }

        [ForeignKey("TutorProfileId")]
        public TutorProfile? TutorProfile { get; set; }

        [Required]
        [StringLength(20)]
        public string DayOfWeek { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}