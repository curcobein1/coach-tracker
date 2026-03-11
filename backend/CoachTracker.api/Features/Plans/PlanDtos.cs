namespace CoachTracker.Api.Features.Plans;

public class TrainingPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<TrainingPlanDayDto> Days { get; set; } = new();
}

public class TrainingPlanDayDto
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public string Name { get; set; } = "";
    public List<TrainingPlanItemDto> Items { get; set; } = new();
}

public class TrainingPlanItemDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public int OrderIndex { get; set; }
    public int TargetSets { get; set; }
    public int? TargetReps { get; set; }
    public int? TargetRestSeconds { get; set; }
    public string? Notes {get; set;}
}