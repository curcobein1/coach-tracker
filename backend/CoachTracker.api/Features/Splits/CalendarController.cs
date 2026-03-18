using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Data;

namespace CoachTracker.Api.Features.Splits;

[ApiController]
[Route("api/calendar")]
public class CalendarController : ControllerBase
{
    private readonly AppDbContext _db;

    public CalendarController(AppDbContext db) => _db = db;

    [HttpGet("day/{date}")]
    public async Task<IActionResult> GetDay(string date)
    {
        var planned = await _db.PlannedCalendarDays
            .Include(p => p.SplitDay)
                .ThenInclude(sd => sd.Split)
            .Include(p => p.SplitDay)
                .ThenInclude(sd => sd.Exercises.OrderBy(e => e.OrderIndex))
                    .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(p => p.Date == DateOnly.Parse(date));
        if (planned == null) return Ok(new { splitDay = (object?)null, exercises = Array.Empty<object>() });

        var exercises = planned.SplitDay.Exercises.OrderBy(e => e.OrderIndex).Select(e => new
        {
            e.ExerciseId,
            exerciseName = e.Exercise.Name,
            e.TargetSets,
            e.TargetRepRange
        }).ToList();
        return Ok(new
        {
            splitDay = new { id = planned.SplitDay.Id, name = planned.SplitDay.Name, splitName = planned.SplitDay.Split.Name },
            exercises
        });
    }

    [HttpPut("day/{date}")]
    public async Task<IActionResult> AssignDay(string date, [FromQuery] int splitDayId)
    {
        var dayKey = DateOnly.Parse(date);
        var splitDay = await _db.SplitDays.FindAsync(splitDayId);
        if (splitDay == null) return BadRequest(new { message = "Split day not found" });

        var existing = await _db.PlannedCalendarDays.FirstOrDefaultAsync(p => p.Date == dayKey);
        if (existing != null)
        {
            existing.SplitDayId = splitDayId;
            await _db.SaveChangesAsync();
            return Ok(new { date = date, splitDayId });
        }
        _db.PlannedCalendarDays.Add(new PlannedCalendarDay { Date = dayKey, SplitDayId = splitDayId });
        await _db.SaveChangesAsync();
        return Ok(new { date = date, splitDayId });
    }

    [HttpDelete("day/{date}")]
    public async Task<IActionResult> ClearDay(string date)
    {
        var dayKey = DateOnly.Parse(date);
        var existing = await _db.PlannedCalendarDays.FirstOrDefaultAsync(p => p.Date == dayKey);
        if (existing == null) return Ok(new { date, cleared = true });
        _db.PlannedCalendarDays.Remove(existing);
        await _db.SaveChangesAsync();
        return Ok(new { date, cleared = true });
    }
}
