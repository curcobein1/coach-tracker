using CoachTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Features.Nutrition;
using CoachTracker.Api.Features.Exercises;
using CoachTracker.Api.Features.Splits;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=coachtracker.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<UsdaFoodService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await EnsureNutritionMicrosColumnAsync(db);
    await SeedSplitDayTemplatesAsync(db);
}

app.UseCors("frontend");
app.UseHttpsRedirection();
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// keep this off for now while local setup is being stabilized
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();

static async Task EnsureNutritionMicrosColumnAsync(AppDbContext db)
{
    // We can't always run EF migrations while the API is already running (locked binaries),
    // so ensure the column exists at runtime as a lightweight, safe schema patch.
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE NutritionFoodLogs ADD COLUMN MicrosJson TEXT NULL;");
    }
    catch
    {
        // Column likely already exists; ignore.
    }
}

static async Task SeedSplitDayTemplatesAsync(AppDbContext db)
{
    // 1) Define which exercises go with which split day (by names).
    //    You fill this out however you like.
    var template = new Dictionary<(string splitName, string dayName), string[]>
    {
        // Example: PPL split
        { ("push / pull / legs", "push"), new[] { "incline sm press", "oh press","pec-deck","cable cuff lraise", "seated cable triceps oh extension", "jm press" } },
        { ("push / pull / legs", "pull"), new[] { "weighted p-up", "l_pd(wg)", "seated incline curl", "upper-back row", "kelso shrug", "narrow grip latfocusedsinglearm row", "preachers", "buttaflies" } },
        { ("push / pull / legs", "legs"), new[] { "leg xtension", "sm bulgarians", "hipaBBductor", "hipadDuctor", "baxktension", "calf"  } },
        { ("upper / lower", "upper"), new[] { "weighted p-up", "weighteddips", "incline sm press", "upper-back row", "cable cuff lraise", "buttaflies", "seated cable triceps oh extension", "preachers"  } },
        { ("upper / lower", "lower"), new[] { "leg xtension", "sm bulgarians", "hipaBBductor", "hipadDuctor", "baxktension", "calf" } },
        { ("torso / limbs", "torso"), new[] { "incline sm press", "l_pd(wg)", "oh press", "upper-back row", "hipthrust", "pec-deck", "cable crunch" } },
        { ("torso / limbs", "limbs"), new[] { "leg xtension", "takanakas", "seated cable triceps oh extension", "seated incline curl", "forearms(r/k)", "singlearm tpushdown", "preachers"  } },
        { ("anterior / posterior", "anterior"), new[] { "bushi", "horshi", "dogshi" } },
        { ("anterior / posterior", "posterior"), new[] { "yadi", "yaddi", "yadda" } },
    };

    if (template.Count == 0) return;

    // 2) Load current splits/days/exercises once
    var splits = await db.Splits
        .Include(s => s.Days)
            .ThenInclude(d => d.Exercises)
        .ToListAsync();
    var allExercises = await db.Exercises.ToListAsync();

    foreach (var ((splitName, dayName), exerciseNames) in template)
    {
        var split = splits.FirstOrDefault(s =>
            s.Name.Equals(splitName, StringComparison.OrdinalIgnoreCase));
        if (split == null) continue;

        var day = split.Days.FirstOrDefault(d =>
            d.Name.Equals(dayName, StringComparison.OrdinalIgnoreCase));
        if (day == null) continue;

        // Skip if this day already has a template defined
        if (day.Exercises.Any()) continue;

        var selectedExercises = allExercises
            .Where(e => exerciseNames.Contains(e.Name))
            .ToList();

        var order = 0;
        foreach (var ex in selectedExercises)
        {
            db.SplitDayExercises.Add(new SplitDayExercise
            {
                SplitDayId = day.Id,
                ExerciseId = ex.Id,
                OrderIndex = order++,
                TargetSets = ex.DefaultPlannedSets ?? 3,
                TargetRepRange = "8-12"
            });
        }
    }

    if (db.ChangeTracker.HasChanges())
    {
        await db.SaveChangesAsync();
    }
}