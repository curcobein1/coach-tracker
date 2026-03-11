namespace CoachTracker.Api.Features.Plans;

public class TrainingPlanDay
{
    public int Id { get; set; }
    public int TrainingPlanId { get; set; }
    public TrainingPlan TrainingPlan { get; set; } = null!;

    public int DayOfWeek { get; set; }
    public string Name { get; set; } = "";

    public List<TrainingPlanItem> Items { get; set; } = new();
}