namespace CoachTracker.Api.Features.Plans;

public class TrainingPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public List<TrainingPlanDay> Days { get; set; } = [];
}