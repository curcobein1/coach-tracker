namespace CoachTracker.Api.Features.Workouts;

public class LogSetDto
{
    public string ExerciseName { get; set; } = string.Empty;
    public double Weight { get; set; }
    public int Reps { get; set; }
    public int Rir { get; set; }

}
