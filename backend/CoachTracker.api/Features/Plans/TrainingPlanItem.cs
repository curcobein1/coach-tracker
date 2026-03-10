using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Plans;

public class TrainingPlanItem
{
    public int Id { get; set; }

    public int TrainingPlanDayId { get; set; }
    public TrainingPlanDay TrainingPlanDay { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int OrderIndex { get; set; }
    public int? TargetSets { get; set; }
    public string? TargetReps { get; set; }
    public int? TargetRestSeconds { get; set; }
    public string? Notes { get; set; }
}