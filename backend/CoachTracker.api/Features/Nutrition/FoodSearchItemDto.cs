namespace CoachTracker.Api.Features.Nutrition;

public class FoodSearchItemDto
{
    public int FdcId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? FoodCategory { get; set; }
    public string? BrandName {get; set; }

}