using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Data;
using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Workouts;

public class FinalizeWorkoutDto
{
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public List<FinalizeWorkoutSetDto> Sets { get; set; } = new();
}

public class FinalizeWorkoutSetDto
{
    public int? ExerciseId { get; set; }
    public string? ExerciseName { get; set; }
    public int SetNumber { get; set; }
    public double Weight { get; set; }
    public int Reps { get; set; }
    public int? Rir { get; set; }
    public string? FormQuality { get; set; }
}

[ApiController]
[Route("api/workouts")]
public class WorkoutsController : ControllerBase
{
    private readonly AppDbContext _db;

    public WorkoutsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{dateStr}")]
    public async Task<IActionResult> GetWorkoutByDate(string dateStr)
    {
        if (!DateOnly.TryParse(dateStr, out var date))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        var workout = await _db.DailyWorkouts
            .Include(dw => dw.Sets)
            .ThenInclude(s => s.Exercise)
            .FirstOrDefaultAsync(dw => dw.Date == date);

        if (workout == null)
        {
            return Ok(new { date, sets = new List<object>() });
        }

        var result = new
        {
            date = workout.Date,
            notes = workout.Notes,
            sets = workout.Sets.OrderBy(s => s.SetNumber).Select(s => new
            {
                id = s.Id,
                exerciseId = s.ExerciseId,
                exerciseName = s.Exercise.Name,
                setNumber = s.SetNumber,
                weight = s.Weight,
                reps = s.Reps,
                rir = s.Rir,
                formQuality = s.FormQuality
            }).ToList()
        };

        return Ok(result);
    }

    [HttpPost("finalize")]
    public async Task<IActionResult> FinalizeWorkout([FromBody] FinalizeWorkoutDto dto)
    {
        // 1. Find or create the DailyWorkout
        var workout = await _db.DailyWorkouts
            .Include(dw => dw.Sets)
            .FirstOrDefaultAsync(dw => dw.Date == dto.Date);

        if (workout == null)
        {
            workout = new DailyWorkout { Date = dto.Date, Notes = dto.Notes };
            _db.DailyWorkouts.Add(workout);
        }
        else
        {
            workout.Notes = dto.Notes;
            // Overwrite sets entirely to reflect the exact state sent from frontend
            _db.WorkoutSets.RemoveRange(workout.Sets);
            workout.Sets.Clear();
        }

        // 2. Process Sets
        foreach (var setDto in dto.Sets)
        {
            Exercise? exercise = null;

            if (setDto.ExerciseId.HasValue)
            {
                exercise = await _db.Exercises.FindAsync(setDto.ExerciseId.Value);
            }
            
            // Auto-create custom exercises!
            if (exercise == null && !string.IsNullOrWhiteSpace(setDto.ExerciseName))
            {
                var trimmedName = setDto.ExerciseName.Trim();
                // Check if we actually have one by name
                exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Name.ToLower() == trimmedName.ToLower());
                
                if (exercise == null)
                {
                    exercise = new Exercise
                    {
                        Name = trimmedName,
                        MovementPatternTag = "Custom",
                        PrimaryMuscle = "Unknown",
                        Equipment = "Unknown"
                    };
                    _db.Exercises.Add(exercise);
                    await _db.SaveChangesAsync(); // Save immediately to generate Id
                }
            }

            if (exercise != null)
            {
                var set = new WorkoutSet
                {
                    DailyWorkoutDate = workout.Date,
                    ExerciseId = exercise.Id,
                    SetNumber = setDto.SetNumber,
                    Weight = setDto.Weight,
                    Reps = setDto.Reps,
                    Rir = setDto.Rir,
                    FormQuality = setDto.FormQuality,
                    LoggedAt = DateTime.UtcNow
                };
                workout.Sets.Add(set);
                _db.WorkoutSets.Add(set);
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Workout finalized perfectly." });
    }
}