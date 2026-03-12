using CoachTracker.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoachTracker.Api.Features.Storage;

[ApiController]
[Route("api/storage")]
public class StorageController : ControllerBase
{
    private readonly AppDbContext _db;

    public StorageController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var entity = await _db.KeyValues.FirstOrDefaultAsync(x => x.Key == key);
        if (entity == null) return Ok(new { key, value = (object?)null });
        return Ok(new { key, json = entity.Json, updatedAtUtc = entity.UpdatedAtUtc });
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Put(string key, [FromBody] object body)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var now = DateTime.UtcNow;

        var entity = await _db.KeyValues.FirstOrDefaultAsync(x => x.Key == key);
        if (entity == null)
        {
            entity = new KeyValueEntry { Key = key, Json = json, UpdatedAtUtc = now };
            _db.KeyValues.Add(entity);
        }
        else
        {
            entity.Json = json;
            entity.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}

