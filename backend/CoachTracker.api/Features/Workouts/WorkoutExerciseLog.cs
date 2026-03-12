using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Workouts;

public class WorkoutExerciseLog
{
    public int Id { get; set; }
    public int WorkoutSessionId { get; set; }
    public WorkoutSession WorkoutSession { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; } = null;

    public List<WorkoutSetLog> Sets { get; set; } = new();
}