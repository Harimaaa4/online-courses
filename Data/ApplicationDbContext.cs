using Microsoft.EntityFrameworkCore;
using online_courses.Entities;

namespace online_courses.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserDb> Users { get; set; }
        public DbSet<CategoryDb> Categories { get; set; } // Бывшие Countries
        public DbSet<CourseDb> Courses { get; set; }       // Бывшие Tours
    }
}