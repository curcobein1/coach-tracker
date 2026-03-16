namespace CoachTracker.Api.Features.Exercises;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Equipment { get; set; }
    public string? MovementPatternTag { get; set; }
    public string? PrimaryMuscle { get; set; }
    public string? SecondaryMuscles { get; set; }
    public int? DefaultPlannedSets { get; set; }
}