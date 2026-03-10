namespace CoachTracker.Api.Features.Nutrition;

public class FoodDetailDto
{
    public int FdcId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? FoodCategory { get; set; }

    public FoodSummaryDto Summary { get; set; } = new();

    public FoodMicronutrientsDto Micronutrients { get; set; } = new();

    public List<NutrientItemDto> Other { get; set; } = new();
}

public class FoodSummaryDto
{
    public double? Calories { get; set; }
    public double? Protein { get; set; }
    public double? Carbs { get; set; }
    public double? Fat { get; set; }
    public double? Fiber { get; set; }
    public double? Sugar { get; set; }
    public double? Sodium { get; set; }
}

public class FoodMicronutrientsDto
{
    public List<NutrientItemDto> Vitamins { get; set; } = new();
    public List<NutrientItemDto> MajorMinerals { get; set; } = new();
    public List<NutrientItemDto> TraceMinerals { get; set; } = new();
}

public class NutrientItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public double? Value { get; set; }
}