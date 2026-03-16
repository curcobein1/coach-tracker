using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Data;
using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Splits;

[ApiController]
[Route("api/splits")]
public class SplitsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SplitsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var list = await _db.Splits
            .Include(s => s.Days)
            .OrderBy(s => s.Name)
            .Select(s => new SplitListItemDto { Id = s.Id, Name = s.Name, DayCount = s.Days.Count })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var split = await _db.Splits
            .Include(s => s.Days.OrderBy(d => d.OrderIndex))
                .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
                    .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (split == null) return NotFound();

        var dto = new SplitDetailDto
        {
            Id = split.Id,
            Name = split.Name,
            Days = split.Days.OrderBy(d => d.OrderIndex).Select(d => new SplitDayDto
            {
                Id = d.Id,
                Name = d.Name,
                OrderIndex = d.OrderIndex,
                Exercises = d.Exercises.OrderBy(e => e.OrderIndex).Select(e => new SplitDayExerciseDto
                {
                    Id = e.Id,
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.Exercise.Name,
                    OrderIndex = e.OrderIndex,
                    TargetSets = e.TargetSets,
                    TargetRepRange = e.TargetRepRange,
                    Notes = e.Notes
                }).ToList()
            }).ToList()
        };
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSplitDto dto)
    {
        var split = new Split { Name = (dto.Name ?? "").Trim() };
        _db.Splits.Add(split);
        await _db.SaveChangesAsync();
        return Ok(new SplitListItemDto { Id = split.Id, Name = split.Name, DayCount = 0 });
    }

    [HttpPost("{splitId:int}/days")]
    public async Task<IActionResult> AddDay(int splitId, [FromBody] AddSplitDayDto dto)
    {
        var split = await _db.Splits.Include(s => s.Days).FirstOrDefaultAsync(s => s.Id == splitId);
        if (split == null) return NotFound();
        var order = split.Days.Any() ? split.Days.Max(d => d.OrderIndex) + 1 : 0;
        var day = new SplitDay { SplitId = splitId, Name = (dto.Name ?? "").Trim(), OrderIndex = order };
        _db.SplitDays.Add(day);
        await _db.SaveChangesAsync();
        return Ok(new SplitDayDto { Id = day.Id, Name = day.Name, OrderIndex = day.OrderIndex, Exercises = new List<SplitDayExerciseDto>() });
    }

    [HttpPost("days/{dayId:int}/exercises")]
    public async Task<IActionResult> AddExercise(int dayId, [FromBody] AddSplitDayExerciseDto dto)
    {
        var day = await _db.SplitDays.Include(d => d.Exercises).FirstOrDefaultAsync(d => d.Id == dayId);
        if (day == null) return NotFound();
        var exercise = await _db.Exercises.FindAsync(dto.ExerciseId);
        if (exercise == null) return BadRequest(new { message = "Exercise not found" });
        var order = day.Exercises.Any() ? day.Exercises.Max(e => e.OrderIndex) + 1 : 0;
        var item = new SplitDayExercise
        {
            SplitDayId = dayId,
            ExerciseId = dto.ExerciseId,
            OrderIndex = order,
            TargetSets = dto.TargetSets,
            TargetRepRange = dto.TargetRepRange,
            Notes = dto.Notes
        };
        _db.SplitDayExercises.Add(item);
        await _db.SaveChangesAsync();
        return Ok(new SplitDayExerciseDto { Id = item.Id, ExerciseId = item.ExerciseId, ExerciseName = exercise.Name, OrderIndex = item.OrderIndex, TargetSets = item.TargetSets, TargetRepRange = item.TargetRepRange, Notes = item.Notes });
    }

    [HttpDelete("day-exercises/{id:int}")]
    public async Task<IActionResult> RemoveExercise(int id)
    {
        var item = await _db.SplitDayExercises.FindAsync(id);
        if (item == null) return NotFound();
        _db.SplitDayExercises.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}
