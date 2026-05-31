using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TopStudentsTutoringPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}