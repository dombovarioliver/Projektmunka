using DiplomaFit.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiplomaFit.Api.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // === Táblák (DbSet-ek) =====================================

        public DbSet<Case> Cases => Set<Case>();
        public DbSet<DietPlan> DietPlans => Set<DietPlan>();
        public DbSet<Food> Foods => Set<Food>();
        public DbSet<FoodMealType> FoodMealTypes => Set<FoodMealType>();
        public DbSet<Meal> Meals => Set<Meal>();
        public DbSet<MealItem> MealItems => Set<MealItem>();
        public DbSet<Exercise> Exercises { get; set; } = null!;

        // ===========================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Exercise>(entity =>
            {
                entity.ToTable("Exercises");

                entity.HasKey(e => e.ExerciseId);

                entity.Property(e => e.NameHu)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.NameEn)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.PrimaryMuscleGroup)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.PrimaryMuscleSubgroup)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.MovementType)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Pattern)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Equipment)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.PushPullCategory)
                      .IsRequired()
                      .HasMaxLength(20);
            });


            // ---- DietPlan -----------------------------------------
            // EF magától nem biztos, hogy PlanId-t kulcsnak veszi, ezért rögzítjük
            modelBuilder.Entity<DietPlan>()
                .HasKey(p => p.PlanId);

            modelBuilder.Entity<DietPlan>()
                .HasMany(p => p.Cases)
                .WithOne(c => c.Plan)
                .HasForeignKey(c => c.PlanId);

            modelBuilder.Entity<DietPlan>()
                .HasMany(p => p.Meals)
                .WithOne(m => m.Plan)
                .HasForeignKey(m => m.PlanId);

            // ---- Case ---------------------------------------------
            modelBuilder.Entity<Case>()
                .HasKey(c => c.CaseId);

            // (kapcsolat a Plan-hez fent van a DietPlan résznél)

            // ---- Food + FoodMealType (kapocs tábla) ---------------
            modelBuilder.Entity<Food>()
                .HasKey(f => f.FoodId);

            modelBuilder.Entity<FoodMealType>()
                .HasKey(fmt => new { fmt.FoodId, fmt.MealType });

            modelBuilder.Entity<FoodMealType>()
                .HasOne(fmt => fmt.Food)
                .WithMany(f => f.MealTypes)
                .HasForeignKey(fmt => fmt.FoodId);

            // ---- Meal ---------------------------------------------
            modelBuilder.Entity<Meal>()
                .HasKey(m => m.MealId);

            modelBuilder.Entity<Meal>()
                .HasOne(m => m.Plan)
                .WithMany(p => p.Meals)
                .HasForeignKey(m => m.PlanId);

            modelBuilder.Entity<Meal>()
                .HasMany(m => m.Items)
                .WithOne(i => i.Meal)
                .HasForeignKey(i => i.MealId);

            // ---- MealItem -----------------------------------------
            modelBuilder.Entity<MealItem>()
                .HasKey(mi => mi.MealItemId);

            modelBuilder.Entity<MealItem>()
                .HasOne(mi => mi.Food)
                .WithMany()
                .HasForeignKey(mi => mi.FoodId);
        }
    }
}
