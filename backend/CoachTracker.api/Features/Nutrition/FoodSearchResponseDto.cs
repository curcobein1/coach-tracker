namespace CoachTracker.Api.Features.Nutrition;

public class FoodSearchResponseDto
{
    public string Query { get; set; } = string.Empty;
    public int PageSize { get; set; }
    public List<FoodSearchItemDto> Foods { get; set; } = new();
}