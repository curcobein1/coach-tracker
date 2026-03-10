using CoachTracker.Api.Features.Exercises;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Features.Plans;

namespace CoachTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<TrainingPlan> TrainingPlans => Set<TrainingPlan>();
    public DbSet<TrainingPlanDay> TrainingPlanDays => Set<TrainingPlanDay>();
    public DbSet<TrainingPlanItem> TrainingPlanItems => Set<TrainingPlanItem>();
}