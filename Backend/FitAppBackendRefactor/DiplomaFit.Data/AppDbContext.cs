using Microsoft.EntityFrameworkCore;
using DiplomaFit.Model.Entities;

namespace DiplomaFit.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
