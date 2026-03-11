namespace CoachTracker.Api.Features.Workouts;

public class WorkoutExerciseLog
{
public int Id { get; set; }
public int WorkoutSessionId {get; set;}
public WorkoutSession WorkoutSession {get; set;} = null;

public int ExerciseId { get; set;}

public List <WorkoutSetLog> Sets {get; set;} = new();
}