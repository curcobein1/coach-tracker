using CoachTracker.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CoachTracker.Api.Features.Admin;

[ApiController]
[Route("api/admin/db")]
public class AdminDbController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminDbController(AppDbContext db) => _db = db;

    [HttpGet("tables")]
    public async Task<IActionResult> GetTables()
    {
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
        var names = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }
        return Ok(new { tables = names });
    }

    [HttpGet("table/{table}/schema")]
    public async Task<IActionResult> GetSchema(string table)
    {
        var safe = await EnsureTableExists(table);
        if (safe == null) return NotFound(new { message = "Table not found" });

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{safe}\");";
        var cols = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cols.Add(new
            {
                cid = reader.GetInt32(0),
                name = reader.GetString(1),
                type = reader.GetString(2),
                notnull = reader.GetInt32(3) == 1,
                dflt_value = reader.IsDBNull(4) ? null : reader.GetValue(4)?.ToString(),
                pk = reader.GetInt32(5) == 1
            });
        }
        return Ok(new { table = safe, columns = cols });
    }

    [HttpGet("table/{table}/rows")]
    public async Task<IActionResult> GetRows(string table, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var safe = await EnsureTableExists(table);
        if (safe == null) return NotFound(new { message = "Table not found" });

        limit = Math.Clamp(limit, 1, 500);
        offset = Math.Max(0, offset);

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT * FROM \"{safe}\" LIMIT $limit OFFSET $offset;";
        cmd.Parameters.Add(new SqliteParameter("$limit", limit));
        cmd.Parameters.Add(new SqliteParameter("$offset", offset));

        await using var reader = await cmd.ExecuteReaderAsync();
        var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
        var rows = new List<object?[]>();
        while (await reader.ReadAsync())
        {
            var arr = new object[reader.FieldCount];
            reader.GetValues(arr);
            rows.Add(arr);
        }
        return Ok(new { table = safe, columns = columnNames, rows, limit, offset });
    }

    public class SqlRequest
    {
        public string Sql { get; set; } = string.Empty;
    }

    [HttpPost("sql")]
    public async Task<IActionResult> RunSql([FromBody] SqlRequest req)
    {
        var sql = (req.Sql ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sql)) return BadRequest(new { message = "SQL is required" });

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        // Try query first; fallback to non-query
        try
        {
            await using var reader = await cmd.ExecuteReaderAsync();
            var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
            var rows = new List<object?[]>();
            while (await reader.ReadAsync())
            {
                var arr = new object[reader.FieldCount];
                reader.GetValues(arr);
                rows.Add(arr);
            }
            return Ok(new { kind = "rows", columns = columnNames, rows });
        }
        catch
        {
            var affected = await cmd.ExecuteNonQueryAsync();
            return Ok(new { kind = "nonQuery", rowsAffected = affected });
        }
    }

    private async Task<string?> EnsureTableExists(string table)
    {
        if (string.IsNullOrWhiteSpace(table)) return null;
        // reject suspicious names
        if (table.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '_' ))) return null;

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$name;";
        var p = cmd.CreateParameter();
        p.ParameterName = "$name";
        p.Value = table;
        cmd.Parameters.Add(p);
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }
}

