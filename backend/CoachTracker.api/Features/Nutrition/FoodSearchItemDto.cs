namespace CoachTracker.Api.Features.Nutrition;

public class FoodSearchItemDto
{
    public int FdcId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? FoodCategory { get; set; }
    public string? BrandName { get; set; }

    public double? Calories { get; set; }
    public double? Protein { get; set; }
    public double? Carbs { get; set; }
    public double? Fat { get; set; }
}