namespace CoachTracker.Api.Features.Workouts;

public class WorkoutSetLog
{
    public int Id { get; set; }

    public int WorkoutExerciseLogId { get; set; }
    public WorkoutExerciseLog WorkoutExerciseLog {get; set;} = null!;
    public int SetNumber {get; set; }
    public double Weight { get; set; }
    public int Reps { get; set; }
    public int? Rir { get; set; } // Reps in Reserve
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}