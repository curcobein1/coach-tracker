namespace CoachTracker.Api.Features.Splits;

public class SplitListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DayCount { get; set; }
}

public class SplitDayExerciseDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = "";
    public int OrderIndex { get; set; }
    public int TargetSets { get; set; }
    public string? TargetRepRange { get; set; }
    public string? Notes { get; set; }
}

public class SplitDayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int OrderIndex { get; set; }
    public List<SplitDayExerciseDto> Exercises { get; set; } = new();
}

public class SplitDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<SplitDayDto> Days { get; set; } = new();
}

public class CreateSplitDto
{
    public string Name { get; set; } = "";
}

public class AddSplitDayDto
{
    public string Name { get; set; } = "";
}

public class AddSplitDayExerciseDto
{
    public int ExerciseId { get; set; }
    public int TargetSets { get; set; }
    public string? TargetRepRange { get; set; }
    public string? Notes { get; set; }
}
