using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Features.Splits;
using CoachTracker.Api.Features.Workouts;
using CoachTracker.Api.Features.Exercises;
using CoachTracker.Api.Features.Nutrition;
using CoachTracker.Api.Features.Storage;

namespace CoachTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<Split> Splits { get; set; }
    public DbSet<SplitDay> SplitDays { get; set; }
    public DbSet<SplitDayExercise> SplitDayExercises { get; set; }
    public DbSet<PlannedCalendarDay> PlannedCalendarDays { get; set; }
    public DbSet<DailyWorkout> DailyWorkouts { get; set; }
    public DbSet<WorkoutSet> WorkoutSets { get; set; }
    public DbSet<NutritionFoodLog> NutritionFoodLogs { get; set; }
    public DbSet<NutritionUsual> NutritionUsuals { get; set; }
    public DbSet<KeyValueEntry> KeyValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KeyValueEntry>().HasKey(x => x.Key);

        modelBuilder.Entity<NutritionFoodLog>().HasKey(x => x.Id);
        modelBuilder.Entity<NutritionFoodLog>().HasIndex(x => x.DayKey);

        modelBuilder.Entity<NutritionUsual>().HasKey(x => x.FdcId);
        modelBuilder.Entity<NutritionUsual>().Property(x => x.FdcId).ValueGeneratedNever();
        modelBuilder.Entity<NutritionUsual>().HasIndex(x => x.UpdatedAtUtc);

        modelBuilder.Entity<PlannedCalendarDay>().HasIndex(x => x.Date).IsUnique();

        modelBuilder.Entity<DailyWorkout>().HasKey(x => x.Date);

        modelBuilder.Entity<WorkoutSet>()
            .HasOne(x => x.DailyWorkout)
            .WithMany(dw => dw.Sets)
            .HasForeignKey(x => x.DailyWorkoutDate);

        // --- SEED DATA ---
        
        // 1. Exercises
        modelBuilder.Entity<Exercise>().HasData(
            new Exercise { Id = 1, Name = "Bench Press", Equipment = "Barbell", MovementPatternTag = "Horizontal Press", PrimaryMuscle = "Pectorals", DefaultPlannedSets = 3 },
            new Exercise { Id = 2, Name = "Incline Dumbbell Press", Equipment = "Dumbbell", MovementPatternTag = "Incline Press", PrimaryMuscle = "Upper Pectorals", DefaultPlannedSets = 3 },
            new Exercise { Id = 3, Name = "Push-ups", Equipment = "Bodyweight", MovementPatternTag = "Horizontal Press", PrimaryMuscle = "Pectorals", DefaultPlannedSets = 3 },
            
            new Exercise { Id = 4, Name = "Pull-ups", Equipment = "Bodyweight", MovementPatternTag = "Vertical Pull", PrimaryMuscle = "Latissimus Dorsi", DefaultPlannedSets = 3 },
            new Exercise { Id = 5, Name = "Lat Pulldown", Equipment = "Machine", MovementPatternTag = "Vertical Pull", PrimaryMuscle = "Latissimus Dorsi", DefaultPlannedSets = 3 },
            new Exercise { Id = 6, Name = "Bent-over Row", Equipment = "Barbell", MovementPatternTag = "Horizontal Pull", PrimaryMuscle = "Rhomboids", DefaultPlannedSets = 3 },

            new Exercise { Id = 7, Name = "Squats", Equipment = "Barbell", MovementPatternTag = "Squat", PrimaryMuscle = "Quadriceps", DefaultPlannedSets = 3 },
            new Exercise { Id = 8, Name = "Romanian Deadlift", Equipment = "Barbell", MovementPatternTag = "Hinge", PrimaryMuscle = "Hamstrings", DefaultPlannedSets = 3 },
            new Exercise { Id = 9, Name = "Hip Thrust", Equipment = "Barbell", MovementPatternTag = "Hip Extension", PrimaryMuscle = "Glutes", DefaultPlannedSets = 3 },
            new Exercise { Id = 10, Name = "Leg Press", Equipment = "Machine", MovementPatternTag = "Leg Press", PrimaryMuscle = "Quadriceps", DefaultPlannedSets = 3 },
            new Exercise { Id = 11, Name = "Calf Raises", Equipment = "Machine", MovementPatternTag = "Calf Raise", PrimaryMuscle = "Calves", DefaultPlannedSets = 3 },

            new Exercise { Id = 12, Name = "Overhead Press", Equipment = "Barbell", MovementPatternTag = "Vertical Press", PrimaryMuscle = "Anterior Deltoids", DefaultPlannedSets = 3 },
            new Exercise { Id = 13, Name = "Lateral Raises", Equipment = "Dumbbell", MovementPatternTag = "Shoulder Isolation", PrimaryMuscle = "Lateral Deltoids", DefaultPlannedSets = 3 },

            new Exercise { Id = 14, Name = "Bicep Curls", Equipment = "Dumbbell", MovementPatternTag = "Elbow Flexion", PrimaryMuscle = "Biceps", DefaultPlannedSets = 3 },
            new Exercise { Id = 15, Name = "Tricep Extensions", Equipment = "Cable", MovementPatternTag = "Elbow Extension", PrimaryMuscle = "Triceps", DefaultPlannedSets = 3 },

            new Exercise { Id = 16, Name = "Plank", Equipment = "Bodyweight", MovementPatternTag = "Core Anti-Extension", PrimaryMuscle = "Abdominals", DefaultPlannedSets = 3 },
            new Exercise { Id = 17, Name = "Hanging Leg Raises", Equipment = "Bodyweight", MovementPatternTag = "Core Flexion", PrimaryMuscle = "Abdominals", DefaultPlannedSets = 3 }
        );

        // 2. Splits
        modelBuilder.Entity<Split>().HasData(
            new Split { Id = 1, Name = "Torso / Limbs" },
            new Split { Id = 2, Name = "Upper / Lower" },
            new Split { Id = 3, Name = "Anterior / Posterior" },
            new Split { Id = 4, Name = "Push / Pull / Legs" }
        );

        // 3. Split Days
        modelBuilder.Entity<SplitDay>().HasData(
            // Torso / Limbs
            new SplitDay { Id = 1, SplitId = 1, Name = "Torso", OrderIndex = 0 },
            new SplitDay { Id = 2, SplitId = 1, Name = "Limbs", OrderIndex = 1 },

            // Upper / Lower
            new SplitDay { Id = 3, SplitId = 2, Name = "Upper", OrderIndex = 0 },
            new SplitDay { Id = 4, SplitId = 2, Name = "Lower", OrderIndex = 1 },

            // Anterior / Posterior
            new SplitDay { Id = 5, SplitId = 3, Name = "Anterior", OrderIndex = 0 },
            new SplitDay { Id = 6, SplitId = 3, Name = "Posterior", OrderIndex = 1 },

            // PPL
            new SplitDay { Id = 7, SplitId = 4, Name = "Push", OrderIndex = 0 },
            new SplitDay { Id = 8, SplitId = 4, Name = "Pull", OrderIndex = 1 },
            new SplitDay { Id = 9, SplitId = 4, Name = "Legs", OrderIndex = 2 }
        );
    }
}