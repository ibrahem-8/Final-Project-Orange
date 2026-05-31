using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TopStudentsTutoringPlatform.Models;

namespace TopStudentsTutoringPlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TutorProfile> TutorProfiles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<TutorSubject> TutorSubjects { get; set; }
        public DbSet<EducationalPackage> EducationalPackages { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<TutorAvailability> TutorAvailabilities { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TutorProfile -> ApplicationUser
            builder.Entity<TutorProfile>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<TutorProfile>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> Student
            builder.Entity<Booking>()
                .HasOne(b => b.Student)
                .WithMany()
                .HasForeignKey(b => b.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> TutorProfile
            builder.Entity<Booking>()
                .HasOne(b => b.TutorProfile)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TutorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> Subject
            builder.Entity<Booking>()
                .HasOne(b => b.Subject)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Student
            builder.Entity<Review>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> TutorProfile
            builder.Entity<Review>()
                .HasOne(r => r.TutorProfile)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TutorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Booking
            builder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Booking
            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Student
            builder.Entity<Payment>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> TutorProfile
            builder.Entity<Payment>()
                .HasOne(p => p.TutorProfile)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TutorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Complaint -> SubmittedBy
            builder.Entity<Complaint>()
                .HasOne(c => c.SubmittedBy)
                .WithMany()
                .HasForeignKey(c => c.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Complaint -> Booking
            builder.Entity<Complaint>()
                .HasOne(c => c.Booking)
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // TutorSubject -> TutorProfile
            builder.Entity<TutorSubject>()
                .HasOne(ts => ts.TutorProfile)
                .WithMany(t => t.TutorSubjects)
                .HasForeignKey(ts => ts.TutorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // TutorSubject -> Subject
            builder.Entity<TutorSubject>()
                .HasOne(ts => ts.Subject)
                .WithMany(s => s.TutorSubjects)
                .HasForeignKey(ts => ts.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // EducationalPackage -> TutorProfile
            builder.Entity<EducationalPackage>()
                .HasOne(ep => ep.TutorProfile)
                .WithMany(t => t.EducationalPackages)
                .HasForeignKey(ep => ep.TutorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // TutorAvailability -> TutorProfile
            builder.Entity<TutorAvailability>()
                .HasOne(a => a.TutorProfile)
                .WithMany(t => t.TutorAvailabilities)
                .HasForeignKey(a => a.TutorProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}