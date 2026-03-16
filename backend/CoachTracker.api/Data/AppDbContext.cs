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
            new Exercise { Id = 1, Name = "incline sm press", Equipment = "smith machine", MovementPatternTag = "incline press", PrimaryMuscle = "upperchest", DefaultPlannedSets = 2 },
            new Exercise { Id = 2, Name = "pec-deck", Equipment = "pec-deck mach", MovementPatternTag = "horizontal abbduction", PrimaryMuscle = "chest", DefaultPlannedSets = 2 },
            new Exercise { Id = 3, Name = "oh press", Equipment = "smith machine", MovementPatternTag = "vertical press", PrimaryMuscle = "side-delts", DefaultPlannedSets = 2 },
            
            new Exercise { Id = 4, Name = "weighted p-up", Equipment = "belt", MovementPatternTag = "Vertical Pull", PrimaryMuscle = "lats", DefaultPlannedSets = 3 },
            new Exercise { Id = 5, Name = "l_pd(wg)", Equipment = "machine", MovementPatternTag = "Vertical Pull", PrimaryMuscle = "lats", DefaultPlannedSets = 2 },
            new Exercise { Id = 6, Name = "seated cable triceps oh extension", Equipment = "cable/bench", MovementPatternTag = "overhead extension", PrimaryMuscle = "tceps longhead", DefaultPlannedSets = 2 },

            new Exercise { Id = 7, Name = "cable cuff lraise", Equipment = "cable", MovementPatternTag = "lateral raise", PrimaryMuscle = "side-delts", DefaultPlannedSets = 2 },
            new Exercise { Id = 8, Name = "seated incline curl", Equipment = "dumbell", MovementPatternTag = "curl", PrimaryMuscle = "bceps", DefaultPlannedSets = 2 },
            new Exercise { Id = 9, Name = "upper-back row", Equipment = "machine", MovementPatternTag = "horizontal pull", PrimaryMuscle = "upper-back", DefaultPlannedSets = 2 },
            new Exercise { Id = 10, Name = "kelso shrug", Equipment = "machine", MovementPatternTag = "scapular regression", PrimaryMuscle = "traps", DefaultPlannedSets = 2 },
            new Exercise { Id = 11, Name = "narrow grip latfocusedsinglearm row", Equipment = "cable", MovementPatternTag = "sagital pull", PrimaryMuscle = "lats", DefaultPlannedSets = 2 },

            new Exercise { Id = 12, Name = "preachers", Equipment = "dumbell", MovementPatternTag = "curl", PrimaryMuscle = "bceps", DefaultPlannedSets = 2 },
            new Exercise { Id = 13, Name = "buttaflies", Equipment = "pec-deck machine", MovementPatternTag = "rear-delt iso", PrimaryMuscle = "rear-delts", DefaultPlannedSets = 2 },

            new Exercise { Id = 14, Name = "weighteddips", Equipment = "belt", MovementPatternTag = "dips", PrimaryMuscle = "chest", DefaultPlannedSets = 2 },
            new Exercise { Id = 15, Name = "Tricep Extensions", Equipment = "Cable", MovementPatternTag = "Elbow Extension", PrimaryMuscle = "Triceps", DefaultPlannedSets = 2 },

            new Exercise { Id = 16, Name = "leg xtension", Equipment = "machine", MovementPatternTag = "extension", PrimaryMuscle = "quads", DefaultPlannedSets = 2 },
            new Exercise { Id = 17, Name = "sm bulgarians", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 18, Name = "hipaBBductor", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 19, Name = "hipadDuctor", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 20, Name = "baxktension", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 21, Name = "calf", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 22, Name = "cable crunch", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 23, Name = "singlearm tpushdown", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 24, Name = "forearms(r/k)", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 25, Name = "takanakas", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 26, Name = "bushi", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 27, Name = "horshi", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 28, Name = "dogshi", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 29, Name = "yadi", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 30, Name = "yaddi", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 },
            new Exercise { Id = 31, Name = "yadda", Equipment = "smith machine", MovementPatternTag = "squat pattern", PrimaryMuscle = "legs", DefaultPlannedSets = 2 }

        );

        // 2. Splits
        modelBuilder.Entity<Split>().HasData(
            new Split { Id = 1, Name = "torso / limbs" },
            new Split { Id = 2, Name = "upper / lower" },
            new Split { Id = 3, Name = "anterior / posterior" },
            new Split { Id = 4, Name = "push / pull / legs" }
        );

        // 3. Split Days
        modelBuilder.Entity<SplitDay>().HasData(
            // Torso / Limbs
            new SplitDay { Id = 1, SplitId = 1, Name = "torso", OrderIndex = 0 },
            new SplitDay { Id = 2, SplitId = 1, Name = "limbs", OrderIndex = 1 },

            // Upper / Lower
            new SplitDay { Id = 3, SplitId = 2, Name = "upper", OrderIndex = 0 },
            new SplitDay { Id = 4, SplitId = 2, Name = "lower", OrderIndex = 1 },

            // Anterior / Posterior
            new SplitDay { Id = 5, SplitId = 3, Name = "anterior", OrderIndex = 0 },
            new SplitDay { Id = 6, SplitId = 3, Name = "posterior", OrderIndex = 1 },

            // PPL
            new SplitDay { Id = 7, SplitId = 4, Name = "push", OrderIndex = 0 },
            new SplitDay { Id = 8, SplitId = 4, Name = "pull", OrderIndex = 1 },
            new SplitDay { Id = 9, SplitId = 4, Name = "legs", OrderIndex = 2 }

        );
        modelBuilder.Entity<SplitDay>()
                    .HasMany(d => d.Exercises)
                    .WithOne(e => e.SplitDay)
                    .HasForeignKey(e => e.SplitDayId);
    }
}