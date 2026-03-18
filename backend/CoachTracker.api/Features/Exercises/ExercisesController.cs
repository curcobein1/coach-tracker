using CoachTracker.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoachTracker.Api.Features.Exercises;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ExercisesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Exercise>>> GetAll()
    {
        var exercises = await _db.Exercises
            .OrderBy(e => e.Name)
            .ToListAsync();

        return Ok(exercises);
    }

    [HttpPost]
    public async Task<ActionResult<Exercise>> Create(Exercise exercise)
    {
        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = exercise.Id }, exercise);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Exercise>> Update(int id, Exercise exercise)
    {
        var existing = await _db.Exercises.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = exercise.Name;
        existing.Equipment = exercise.Equipment;
        existing.MovementPatternTag = exercise.MovementPatternTag;
        existing.PrimaryMuscle = exercise.PrimaryMuscle;
        existing.SecondaryMuscles = exercise.SecondaryMuscles;
        existing.DefaultPlannedSets = exercise.DefaultPlannedSets;

        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Exercises.FindAsync(id);
        if (existing == null) return Ok(new { deleted = true });

        _db.Exercises.Remove(existing);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}