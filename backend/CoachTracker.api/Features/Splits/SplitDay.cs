namespace CoachTracker.Api.Features.Splits;

public class SplitDay
{
    public int Id { get; set; }
    public int SplitId { get; set; }
    public Split Split { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public List<SplitDayExercise> Exercises { get; set; } = new();
}
