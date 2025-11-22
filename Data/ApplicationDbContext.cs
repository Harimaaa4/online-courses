using Microsoft.EntityFrameworkCore;
using online_courses.Entities;
using System.Collections.Generic;

namespace online_courses.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserDb> Users { get; set; }
        public DbSet<CountryDb> Countries { get; set; }
        public DbSet<TourDb> Tours { get; set; }
    }
}