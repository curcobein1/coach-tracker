using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Splits;

public class SplitDayExercise
{
    public int Id { get; set; }
    public int SplitDayId { get; set; }
    public SplitDay? SplitDay { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; } = null!;

    public int OrderIndex { get; set; }
    public int TargetSets { get; set; }
    public string? TargetRepRange { get; set; }
    public string? Notes { get; set; }
}
