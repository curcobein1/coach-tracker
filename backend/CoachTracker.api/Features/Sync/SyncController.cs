using CoachTracker.Api.Data;
using CoachTracker.Api.Features.Nutrition;
using CoachTracker.Api.Features.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CoachTracker.Api.Features.Sync;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _db;

    public SyncController(AppDbContext db)
    {
        _db = db;
    }

    public record ImportRequest(
        Dictionary<string, JsonElement>? Kv,
        JsonElement? NutritionDays,
        JsonElement? NutritionUsuals
    );

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportRequest req)
    {
        var now = DateTime.UtcNow;

        // KV mirror
        if (req.Kv != null)
        {
            foreach (var (key, value) in req.Kv)
            {
                var json = value.GetRawText();
                var existing = await _db.KeyValues.FirstOrDefaultAsync(x => x.Key == key);
                if (existing == null)
                    _db.KeyValues.Add(new KeyValueEntry { Key = key, Json = json, UpdatedAtUtc = now });
                else
                {
                    existing.Json = json;
                    existing.UpdatedAtUtc = now;
                }
            }
        }

        // Nutrition days: { "YYYY-MM-DD": { date, foods: [...] }, ... }
        if (req.NutritionDays.HasValue && req.NutritionDays.Value.ValueKind == JsonValueKind.Object)
        {
            foreach (var dayProp in req.NutritionDays.Value.EnumerateObject())
            {
                var dayKey = dayProp.Name;
                if (dayProp.Value.ValueKind != JsonValueKind.Object) continue;

                if (!dayProp.Value.TryGetProperty("foods", out var foodsEl) || foodsEl.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var foodEl in foodsEl.EnumerateArray())
                {
                    // best-effort parse
                    var at = foodEl.TryGetProperty("at", out var atEl) ? atEl.GetString() : null;
                    var loggedAt = DateTime.TryParse(at, out var dt) ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : now;

                    var id = Guid.NewGuid();
                    var fdcId = foodEl.TryGetProperty("fdcId", out var fdcEl) ? fdcEl.GetInt32() : 0;
                    var name = foodEl.TryGetProperty("name", out var nameEl) ? (nameEl.GetString() ?? "") : "";

                    double GetNum(string prop)
                        => foodEl.TryGetProperty(prop, out var x) && x.ValueKind == JsonValueKind.Number ? x.GetDouble() : 0;

                    _db.NutritionFoodLogs.Add(new NutritionFoodLog
                    {
                        Id = id,
                        DayKey = dayKey,
                        FdcId = fdcId,
                        Name = name,
                        Grams = GetNum("grams"),
                        Kcal = GetNum("kcal"),
                        P = GetNum("p"),
                        C = GetNum("c"),
                        F = GetNum("f"),
                        LoggedAtUtc = loggedAt
                    });
                }
            }
        }

        // Nutrition usuals: [ { fdcId, name, grams, kcal, p, c, f }, ...]
        if (req.NutritionUsuals.HasValue && req.NutritionUsuals.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var uEl in req.NutritionUsuals.Value.EnumerateArray())
            {
                if (!uEl.TryGetProperty("fdcId", out var fdcEl) || fdcEl.ValueKind != JsonValueKind.Number) continue;
                var fdcId = fdcEl.GetInt32();

                var name = uEl.TryGetProperty("name", out var nEl) ? (nEl.GetString() ?? "") : "";
                double GetNum(string prop)
                    => uEl.TryGetProperty(prop, out var x) && x.ValueKind == JsonValueKind.Number ? x.GetDouble() : 0;

                var existing = await _db.NutritionUsuals.FirstOrDefaultAsync(x => x.FdcId == fdcId);
                if (existing == null)
                {
                    _db.NutritionUsuals.Add(new NutritionUsual
                    {
                        FdcId = fdcId,
                        Name = name,
                        Grams = GetNum("grams"),
                        Kcal = GetNum("kcal"),
                        P = GetNum("p"),
                        C = GetNum("c"),
                        F = GetNum("f"),
                        UpdatedAtUtc = now
                    });
                }
                else
                {
                    existing.Name = name;
                    existing.Grams = GetNum("grams");
                    existing.Kcal = GetNum("kcal");
                    existing.P = GetNum("p");
                    existing.C = GetNum("c");
                    existing.F = GetNum("f");
                    existing.UpdatedAtUtc = now;
                }
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}

