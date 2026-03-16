using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Workouts;

public class WorkoutSet
{
    public int Id { get; set; }
    
    public DateOnly DailyWorkoutDate { get; set; }
    public DailyWorkout DailyWorkout { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int SetNumber { get; set; }
    public double Weight { get; set; }
    public int Reps { get; set; }
    public int? Rir { get; set; }
    public string? FormQuality { get; set; }
    
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
