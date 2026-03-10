namespace CoachTracker.Api.Features.Exercises;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Equipment { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
}