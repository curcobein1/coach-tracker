namespace CoachTracker.Api.Features.Splits;

public class Split
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SplitDay> Days { get; set; } = new();
}
