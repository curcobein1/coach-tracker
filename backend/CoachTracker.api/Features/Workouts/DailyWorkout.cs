namespace CoachTracker.Api.Features.Workouts;

public class DailyWorkout
{
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public List<WorkoutSet> Sets { get; set; } = new();
}
