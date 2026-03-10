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
}