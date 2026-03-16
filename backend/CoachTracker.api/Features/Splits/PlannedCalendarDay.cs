namespace CoachTracker.Api.Features.Splits;

public class PlannedCalendarDay
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int SplitDayId { get; set; }
    public SplitDay SplitDay { get; set; } = null!;
}
