using CoachTracker.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoachTracker.Api.Features.Nutrition;

[ApiController]
[Route("api/nutrition")]
public class NutritionLogController : ControllerBase
{
    private readonly AppDbContext _db;

    public NutritionLogController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("days/{dayKey}")]
    public async Task<IActionResult> GetDay(string dayKey)
    {
        var foods = await _db.NutritionFoodLogs
            .Where(f => f.DayKey == dayKey)
            .OrderByDescending(f => f.LoggedAtUtc)
            .Select(f => new NutritionFoodLogDto(
                f.Id, f.DayKey, f.FdcId, f.Name, f.Grams, f.Kcal, f.P, f.C, f.F, f.LoggedAtUtc
            ))
            .ToListAsync();

        return Ok(new { dayKey, foods });
    }

    [HttpPost("days/{dayKey}/foods")]
    public async Task<IActionResult> AddFood(string dayKey, [FromBody] CreateNutritionFoodLogRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "name is required" });

        var entity = new NutritionFoodLog
        {
            Id = Guid.NewGuid(),
            DayKey = dayKey,
            FdcId = req.FdcId,
            Name = req.Name,
            Grams = req.Grams,
            Kcal = req.Kcal,
            P = req.P,
            C = req.C,
            F = req.F,
            LoggedAtUtc = DateTime.UtcNow
        };

        _db.NutritionFoodLogs.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new NutritionFoodLogDto(
            entity.Id, entity.DayKey, entity.FdcId, entity.Name, entity.Grams, entity.Kcal, entity.P, entity.C, entity.F, entity.LoggedAtUtc
        ));
    }

    [HttpDelete("days/{dayKey}/foods/{id:guid}")]
    public async Task<IActionResult> DeleteFood(string dayKey, Guid id)
    {
        var entity = await _db.NutritionFoodLogs.FirstOrDefaultAsync(f => f.Id == id && f.DayKey == dayKey);
        if (entity == null) return NotFound();

        _db.NutritionFoodLogs.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [HttpGet("usuals")]
    public async Task<IActionResult> GetUsuals()
    {
        var usuals = await _db.NutritionUsuals
            .OrderByDescending(u => u.UpdatedAtUtc)
            .Select(u => new NutritionUsualDto(
                u.FdcId, u.Name, u.Grams, u.Kcal, u.P, u.C, u.F, u.UpdatedAtUtc
            ))
            .ToListAsync();

        return Ok(new { usuals });
    }

    [HttpPut("usuals/{fdcId:int}")]
    public async Task<IActionResult> UpsertUsual(int fdcId, [FromBody] UpsertNutritionUsualRequest req)
    {
        var now = DateTime.UtcNow;
        var entity = await _db.NutritionUsuals.FirstOrDefaultAsync(u => u.FdcId == fdcId);
        if (entity == null)
        {
            entity = new NutritionUsual
            {
                FdcId = fdcId,
                Name = req.Name,
                Grams = req.Grams,
                Kcal = req.Kcal,
                P = req.P,
                C = req.C,
                F = req.F,
                UpdatedAtUtc = now
            };
            _db.NutritionUsuals.Add(entity);
        }
        else
        {
            entity.Name = req.Name;
            entity.Grams = req.Grams;
            entity.Kcal = req.Kcal;
            entity.P = req.P;
            entity.C = req.C;
            entity.F = req.F;
            entity.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync();
        return Ok(new NutritionUsualDto(entity.FdcId, entity.Name, entity.Grams, entity.Kcal, entity.P, entity.C, entity.F, entity.UpdatedAtUtc));
    }

    [HttpDelete("usuals/{fdcId:int}")]
    public async Task<IActionResult> DeleteUsual(int fdcId)
    {
        var entity = await _db.NutritionUsuals.FirstOrDefaultAsync(u => u.FdcId == fdcId);
        if (entity == null) return NotFound();
        _db.NutritionUsuals.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}

