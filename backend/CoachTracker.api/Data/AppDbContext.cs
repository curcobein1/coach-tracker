using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Features.Plans;
using CoachTracker.Api.Features.Workouts;
using CoachTracker.Api.Features.Exercises;
using CoachTracker.Api.Features.Nutrition;
using CoachTracker.Api.Features.Storage;

namespace CoachTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Exercises
    public DbSet<Exercise> Exercises { get; set; }

    // Plan Domain
    public DbSet<TrainingPlan> TrainingPlans { get; set; }
    public DbSet<TrainingPlanDay> TrainingPlanDays { get; set; }
    public DbSet<TrainingPlanItem> TrainingPlanItems { get; set; }

    // Workout Domain
    public DbSet<WorkoutSession> WorkoutSessions { get; set; }
    public DbSet<WorkoutExerciseLog> WorkoutExerciseLogs { get; set; }
    public DbSet<WorkoutSetLog> WorkoutSetLogs { get; set; }

    // Nutrition Domain
    public DbSet<NutritionFoodLog> NutritionFoodLogs { get; set; }
    public DbSet<NutritionUsual> NutritionUsuals { get; set; }

    // Generic key-value storage (mirror browser localStorage)
    public DbSet<KeyValueEntry> KeyValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KeyValueEntry>().HasKey(x => x.Key);

        modelBuilder.Entity<NutritionFoodLog>().HasKey(x => x.Id);
        modelBuilder.Entity<NutritionFoodLog>()
            .HasIndex(x => x.DayKey);

        modelBuilder.Entity<NutritionUsual>().HasKey(x => x.FdcId);
        modelBuilder.Entity<NutritionUsual>()
            .Property(x => x.FdcId)
            .ValueGeneratedNever();
        modelBuilder.Entity<NutritionUsual>()
            .HasIndex(x => x.UpdatedAtUtc);
    }
}