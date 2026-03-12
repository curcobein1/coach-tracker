using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Data;
using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Workouts;

[ApiController]
[Route("api/workouts")]
public class WorkoutsController : ControllerBase
{
    private readonly AppDbContext _db;

    public WorkoutsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var session = await _db.WorkoutSessions
            .Include(s => s.ExerciseLogs)
                .ThenInclude(el => el.Sets)
            .Include(s => s.ExerciseLogs)
                .ThenInclude(el => el.Exercise)
            .FirstOrDefaultAsync(s => s.Date == today);

        if (session == null)
        {
            return Ok(new
            {
                date = today,
                exercises = new List<object>()
            });
        }

        var result = new
        {
            sessionId = session.Id,
            date = session.Date,
            notes = session.Notes,
            exercises = session.ExerciseLogs.Select(el => new
            {
                exerciseLogId = el.Id,
                exerciseId = el.ExerciseId,
                exerciseName = el.Exercise != null ? el.Exercise.Name : null,
                sets = el.Sets
                    .OrderBy(s => s.SetNumber)
                    .Select(s => new
                    {
                        id = s.Id,
                        setNumber = s.SetNumber,
                        weight = s.Weight,
                        reps = s.Reps,
                        rir = s.Rir,
                        loggedAt = s.LoggedAt
                    })
                    .ToList()
            }).ToList()
        };

        return Ok(result);
    }

    [HttpPost("log-set")]
    public async Task<IActionResult> LogSet([FromBody] LogSetDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ExerciseName))
        {
            return BadRequest(new { message = "ExerciseName is required." });
        }

        var trimmedName = dto.ExerciseName.Trim();
        var exercise = await _db.Exercises
            .FirstOrDefaultAsync(e => e.Name.ToLower() == trimmedName.ToLower());

        if (exercise == null)
        {
            exercise = new Exercise
            {
                Name = trimmedName,
                Group = "Unknown",
                Category = "UserLogged"
            };
            _db.Exercises.Add(exercise);
            await _db.SaveChangesAsync();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        var session = await _db.WorkoutSessions
            .Include(s => s.ExerciseLogs)
            .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(s => s.Date == today);

        if (session == null)
        {
            session = new WorkoutSession
            {
                Date = today,
                Notes = null,
            };
            _db.WorkoutSessions.Add(session);
            await _db.SaveChangesAsync();
        }

        var exerciseLog = await _db.WorkoutExerciseLogs
            .Include(e => e.Sets)
            .FirstOrDefaultAsync(e =>
                e.WorkoutSessionId == session.Id &&
                e.ExerciseId == exercise.Id);

        if (exerciseLog == null)
        {
            exerciseLog = new WorkoutExerciseLog
            {
                WorkoutSessionId = session.Id,
                ExerciseId = exercise.Id,
            };

            _db.WorkoutExerciseLogs.Add(exerciseLog);
            await _db.SaveChangesAsync();
        }

        var nextSetNumber = exerciseLog.Sets.Any()
            ? exerciseLog.Sets.Max(s => s.SetNumber) + 1
            : 1;

        var setLog = new WorkoutSetLog
        {
            WorkoutExerciseLogId = exerciseLog.Id,
            SetNumber = nextSetNumber,
            Weight = dto.Weight,
            Reps = dto.Reps,
            Rir = dto.Rir,
            LoggedAt = DateTime.UtcNow
        };

        _db.WorkoutSetLogs.Add(setLog);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "set saved",
            sessionId = session.Id,
            exerciseLogId = exerciseLog.Id,
            setId= setLog.Id,
            setNumber = setLog.SetNumber,
            weight = setLog.Weight,
            reps = setLog.Reps,
            rir = setLog.Rir
        });
    }

    [HttpDelete("sets/{setId:int}")]
    public async Task<IActionResult> DeleteSet(int setId)
    {
        var set = await _db.WorkoutSetLogs
            .Include(s => s.WorkoutExerciseLog)
            .ThenInclude(e => e.Sets)
            .Include(s => s.WorkoutExerciseLog.WorkoutSession)
            .ThenInclude(ws => ws.ExerciseLogs)
            .FirstOrDefaultAsync(s => s.Id == setId);

        if (set == null) return NotFound();

        var exerciseLog = set.WorkoutExerciseLog;
        var session = exerciseLog.WorkoutSession;

        _db.WorkoutSetLogs.Remove(set);

        // If no more sets for this exercise, remove exercise log
        if (exerciseLog.Sets.Count <= 1)
        {
            _db.WorkoutExerciseLogs.Remove(exerciseLog);
        }

        // If session has no more exercise logs, remove the session
        if (session.ExerciseLogs.Count <= 1 && exerciseLog.Sets.Count <= 1)
        {
            _db.WorkoutSessions.Remove(session);
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}