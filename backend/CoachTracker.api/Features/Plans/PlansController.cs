using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Data;

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

        plan.Days = dto.Days.Select(d => new TrainingPlanDay
        {
            DayOfWeek = d.DayOfWeek,
            Name = d.Name,
            Items = d.Items.Select(i => new TrainingPlanItem
            {
                Id = i.Id,
                ExerciseId = i.ExerciseId,
                OrderIndex= i.OrderIndex,
                TargetSets = i.TargetSets,
                TargetReps = i.TargetReps,
                TargetRestSeconds = i.TargetRestSeconds,
                Notes =i.Notes
            }).ToList()
        }).ToList();

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