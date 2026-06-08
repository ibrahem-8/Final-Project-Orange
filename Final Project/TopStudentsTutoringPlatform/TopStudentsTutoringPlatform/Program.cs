using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TopStudentsTutoringPlatform.Data;
using TopStudentsTutoringPlatform.Models;
using Stripe;

namespace TopStudentsTutoringPlatform
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                // Simple password settings for development/demo
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            var app = builder.Build();

            // Seed roles and admin account
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Admin", "Student", "Tutor" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                string adminEmail = "admin@topstudents.com";
                string adminPassword = "Admin123";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        FullName = "System Admin",
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.Now
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
                string tutorEmail = "tutor@topstudents.com";
                string tutorPassword = "Tutor123";

                var tutorUser = await userManager.FindByEmailAsync(tutorEmail);

                if (tutorUser == null)
                {
                    tutorUser = new ApplicationUser
                    {
                        FullName = "Demo Tutor",
                        UserName = tutorEmail,
                        Email = tutorEmail,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.Now
                    };

                    var result = await userManager.CreateAsync(tutorUser, tutorPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(tutorUser, "Tutor");
                    }
                }
                string studentEmail = "student@topstudents.com";
                string studentPassword = "Student123";

                var studentUser = await userManager.FindByEmailAsync(studentEmail);

                if (studentUser == null)
                {
                    studentUser = new ApplicationUser
                    {
                        FullName = "Demo Student",
                        UserName = studentEmail,
                        Email = studentEmail,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.Now
                    };

                    var result = await userManager.CreateAsync(studentUser, studentPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(studentUser, "Student");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}