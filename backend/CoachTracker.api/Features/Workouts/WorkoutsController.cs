using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Data;

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
                e.ExerciseId == dto.ExerciseId);

        if (exerciseLog == null)
        {
            exerciseLog = new WorkoutExerciseLog
            {
                WorkoutSessionId = session.Id,
                ExerciseId = dto.ExerciseId,
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
}