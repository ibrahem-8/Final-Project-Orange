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
    }
}