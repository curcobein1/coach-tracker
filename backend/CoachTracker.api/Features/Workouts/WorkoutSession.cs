namespace CoachTracker.Api.Features.Workouts;

public class WorkoutSession
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public List<WorkoutExerciseLog> ExerciseLogs { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}