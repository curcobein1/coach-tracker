namespace CoachTracker.Api.Features.Nutrition;

// --- Entities (EF Core) ---

public class NutritionFoodLog
{
    public Guid Id { get; set; }
    public string DayKey { get; set; } = default!; // YYYY-MM-DD

    public int FdcId { get; set; }
    public string Name { get; set; } = default!;

    public double Grams { get; set; }
    public double Kcal { get; set; }
    public double P { get; set; }
    public double C { get; set; }
    public double F { get; set; }

    // Flexible micronutrients payload (keys/values/units), stored as JSON.
    public string? MicrosJson { get; set; }

    public DateTime LoggedAtUtc { get; set; }
}

public class NutritionUsual
{
    public int FdcId { get; set; } // primary key (USDA id)
    public string Name { get; set; } = default!;

    public double Grams { get; set; }
    public double Kcal { get; set; }
    public double P { get; set; }
    public double C { get; set; }
    public double F { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

// --- DTOs for API ---

public record NutritionFoodLogDto(
    Guid Id,
    string DayKey,
    int FdcId,
    string Name,
    double Grams,
    double Kcal,
    double P,
    double C,
    double F,
    string? MicrosJson,
    DateTime LoggedAtUtc
);

public record CreateNutritionFoodLogRequest(
    string DayKey,
    int FdcId,
    string Name,
    double Grams,
    double Kcal,
    double P,
    double C,
    double F,
    string? MicrosJson
);

public record NutritionUsualDto(
    int FdcId,
    string Name,
    double Grams,
    double Kcal,
    double P,
    double C,
    double F,
    DateTime UpdatedAtUtc
);

public record UpsertNutritionUsualRequest(
    int FdcId,
    string Name,
    double Grams,
    double Kcal,
    double P,
    double C,
    double F
);

