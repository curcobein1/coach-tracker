using CoachTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using CoachTracker.Api.Features.Nutrition;

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