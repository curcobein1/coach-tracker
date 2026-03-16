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
    await SeedIfEmptyAsync(db);
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

static async Task SeedIfEmptyAsync(AppDbContext db)
{
    if (await db.Exercises.AnyAsync()) return;
    var exercises = new[]
    {
        new Exercise { Name = "Bench Press", Equipment = "Barbell", MovementPatternTag = "Compound", PrimaryMuscle = "Chest", DefaultPlannedSets = 3 },
        new Exercise { Name = "Squat", Equipment = "Barbell", MovementPatternTag = "Compound", PrimaryMuscle = "Quads", DefaultPlannedSets = 3 },
        new Exercise { Name = "Deadlift", Equipment = "Barbell", MovementPatternTag = "Compound", PrimaryMuscle = "Back", DefaultPlannedSets = 3 },
        new Exercise { Name = "Overhead Press", Equipment = "Barbell", MovementPatternTag = "Compound", PrimaryMuscle = "Shoulders", DefaultPlannedSets = 3 },
        new Exercise { Name = "Barbell Row", Equipment = "Barbell", MovementPatternTag = "Compound", PrimaryMuscle = "Back", DefaultPlannedSets = 3 },
        new Exercise { Name = "Lat Pulldown", Equipment = "Cable", MovementPatternTag = "Isolation", PrimaryMuscle = "Back", DefaultPlannedSets = 3 },
        new Exercise { Name = "Leg Press", Equipment = "Machine", MovementPatternTag = "Compound", PrimaryMuscle = "Quads", DefaultPlannedSets = 3 },
        new Exercise { Name = "Dumbbell Curl", Equipment = "Dumbbell", MovementPatternTag = "Isolation", PrimaryMuscle = "Biceps", DefaultPlannedSets = 3 },
        new Exercise { Name = "Tricep Pushdown", Equipment = "Cable", MovementPatternTag = "Isolation", PrimaryMuscle = "Triceps", DefaultPlannedSets = 3 },
    };
    db.Exercises.AddRange(exercises);
    await db.SaveChangesAsync();
    var split = new Split { Name = "Push / Pull / Legs" };
    db.Splits.Add(split);
    await db.SaveChangesAsync();
    var d1 = new SplitDay { SplitId = split.Id, Name = "Push", OrderIndex = 0 };
    var d2 = new SplitDay { SplitId = split.Id, Name = "Pull", OrderIndex = 1 };
    var d3 = new SplitDay { SplitId = split.Id, Name = "Legs", OrderIndex = 2 };
    db.SplitDays.AddRange(d1, d2, d3);
    await db.SaveChangesAsync();
    var bench = exercises.First(e => e.Name == "Bench Press");
    var ohp = exercises.First(e => e.Name == "Overhead Press");
    var tri = exercises.First(e => e.Name == "Tricep Pushdown");
    db.SplitDayExercises.AddRange(
        new SplitDayExercise { SplitDayId = d1.Id, ExerciseId = bench.Id, OrderIndex = 0, TargetSets = 3, TargetRepRange = "8-12" },
        new SplitDayExercise { SplitDayId = d1.Id, ExerciseId = ohp.Id, OrderIndex = 1, TargetSets = 3, TargetRepRange = "8-12" },
        new SplitDayExercise { SplitDayId = d1.Id, ExerciseId = tri.Id, OrderIndex = 2, TargetSets = 3, TargetRepRange = "10-15" });
    var row = exercises.First(e => e.Name == "Barbell Row");
    var lat = exercises.First(e => e.Name == "Lat Pulldown");
    var curl = exercises.First(e => e.Name == "Dumbbell Curl");
    db.SplitDayExercises.AddRange(
        new SplitDayExercise { SplitDayId = d2.Id, ExerciseId = row.Id, OrderIndex = 0, TargetSets = 3, TargetRepRange = "8-12" },
        new SplitDayExercise { SplitDayId = d2.Id, ExerciseId = lat.Id, OrderIndex = 1, TargetSets = 3, TargetRepRange = "10-12" },
        new SplitDayExercise { SplitDayId = d2.Id, ExerciseId = curl.Id, OrderIndex = 2, TargetSets = 3, TargetRepRange = "10-15" });
    var squat = exercises.First(e => e.Name == "Squat");
    var legPress = exercises.First(e => e.Name == "Leg Press");
    db.SplitDayExercises.AddRange(
        new SplitDayExercise { SplitDayId = d3.Id, ExerciseId = squat.Id, OrderIndex = 0, TargetSets = 3, TargetRepRange = "6-10" },
        new SplitDayExercise { SplitDayId = d3.Id, ExerciseId = legPress.Id, OrderIndex = 1, TargetSets = 3, TargetRepRange = "10-15" });
    await db.SaveChangesAsync();
}