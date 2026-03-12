namespace CoachTracker.Api.Features.Storage;

public class KeyValueEntry
{
    public string Key { get; set; } = default!;
    public string Json { get; set; } = default!;
    public DateTime UpdatedAtUtc { get; set; }
}

