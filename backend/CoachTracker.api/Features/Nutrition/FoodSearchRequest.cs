namespace CoachTracker.Api.Features.Nutrition;

public class FoodSearchRequest{
    public string Query { get; set; } = string.Empty;
    public int PageSize { get; set; } = 10;
}