using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Features.Plans;
using CoachTracker.Api.Features.Workouts;
using CoachTracker.Api.Features.Exercises;

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
}