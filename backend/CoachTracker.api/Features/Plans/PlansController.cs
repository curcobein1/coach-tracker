using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Data;
using CoachTracker.Api.Features.Exercises;

namespace CoachTracker.Api.Features.Plans;

[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlansController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var plan = await _db.TrainingPlans
            .Include(p => p.Days)
            .ThenInclude(d => d.Items)
                .ThenInclude(i => i.Exercise)
            .FirstOrDefaultAsync(p => p.IsActive);

        if (plan == null)
        {
            var emptyPlan = new TrainingPlan
            {
                Name = "Active Plan",
                IsActive = true
            };

            _db.TrainingPlans.Add(emptyPlan);
            await _db.SaveChangesAsync();

            return Ok(new TrainingPlanDto
            {
                Id = emptyPlan.Id,
                Name = emptyPlan.Name,
                Days = new List<TrainingPlanDayDto>()
            });
        }

        var dto = new TrainingPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Days = plan.Days
                .OrderBy(d => d.DayOfWeek)
                .Select(d => new TrainingPlanDayDto
                {
                    Id = d.Id,
                    DayOfWeek = d.DayOfWeek,
                    Name = d.Name,
                    Items = d.Items.Select(i => new TrainingPlanItemDto
                    {
                        Id = i.Id,
                        ExerciseId = i.ExerciseId,
                        ExerciseName = i.Exercise?.Name ?? string.Empty,
                        OrderIndex= i.OrderIndex,
                        TargetSets = i.TargetSets,
                        TargetReps = i.TargetReps,
                        TargetRestSeconds = i.TargetRestSeconds,
                        Notes =i.Notes
                    }).ToList()
                }).ToList()
        };

        return Ok(dto);
    }

    [HttpPut("active")]
    public async Task<IActionResult> UpdateActive([FromBody] TrainingPlanDto dto)
    {
        var plan = await _db.TrainingPlans
            .Include(p => p.Days)
                .ThenInclude(d => d.Items)
            .FirstOrDefaultAsync(p => p.IsActive);

        if (plan == null)
        {
            plan = new TrainingPlan
            {
                Name = dto.Name,
                IsActive = true
            };

            _db.TrainingPlans.Add(plan);
            await _db.SaveChangesAsync();
        }

        plan.Name = dto.Name;

        // Remove old nested graph
        _db.TrainingPlanItems.RemoveRange(plan.Days.SelectMany(d => d.Items));
        _db.TrainingPlanDays.RemoveRange(plan.Days);

        // Rebuild from DTO, resolving/creating Exercises from ExerciseName so
        // template-based and free-text plan items persist correctly.
        var newDays = new List<TrainingPlanDay>();

        foreach (var dayDto in dto.Days)
        {
            var day = new TrainingPlanDay
            {
                DayOfWeek = dayDto.DayOfWeek,
                Name = dayDto.Name,
                Items = new List<TrainingPlanItem>()
            };

            foreach (var itemDto in dayDto.Items)
            {
                // Resolve or create Exercise based on ExerciseName / ExerciseId.
                var exerciseName = (itemDto.ExerciseName ?? string.Empty).Trim();

                Exercise? exercise = null;

                if (itemDto.ExerciseId != 0)
                {
                    exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Id == itemDto.ExerciseId);
                }

                if (exercise is null)
                {
                    if (string.IsNullOrWhiteSpace(exerciseName))
                    {
                        exerciseName = $"Planned Exercise #{itemDto.OrderIndex + 1}";
                    }

                    var normalized = exerciseName.ToLower();

                    exercise = await _db.Exercises
                        .FirstOrDefaultAsync(e => e.Name.ToLower() == normalized);

                    if (exercise is null)
                    {
                        exercise = new Exercise
                        {
                            Name = exerciseName,
                            Group = "Unknown",
                            Category = "Plan"
                        };
                        _db.Exercises.Add(exercise);
                    }
                }

                day.Items.Add(new TrainingPlanItem
                {
                    Exercise = exercise,
                    OrderIndex = itemDto.OrderIndex,
                    TargetSets = itemDto.TargetSets,
                    TargetReps = itemDto.TargetReps,
                    TargetRestSeconds = itemDto.TargetRestSeconds,
                    Notes = itemDto.Notes
                });
            }

            newDays.Add(day);
        }

        plan.Days = newDays;

        await _db.SaveChangesAsync();

        var result = new TrainingPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Days = plan.Days
                .OrderBy(d => d.DayOfWeek)
                .Select(d => new TrainingPlanDayDto
                {
                    Id = d.Id,
                    DayOfWeek = d.DayOfWeek,
                    Name = d.Name,
                    Items = d.Items.Select(i => new TrainingPlanItemDto
                    {
                        Id = i.Id,
                        ExerciseId = i.ExerciseId,
                        OrderIndex= i.OrderIndex,
                        TargetSets = i.TargetSets,
                        TargetReps = i.TargetReps,
                        TargetRestSeconds = i.TargetRestSeconds,
                        Notes =i.Notes
                    }).ToList()
                }).ToList()
        };

        return Ok(result);
    }
}