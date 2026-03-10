using System.Text.Json.Serialization;
using System.Text.Json;

namespace CoachTracker.Api.Features.Nutrition;

public class UsdaSearchResponse
{
    [JsonPropertyName("foods")]
    public List<UsdaFoodItem> Foods { get; set; } = new();
}

public class UsdaFoodItem
{
    [JsonPropertyName("fdcId")]
    public int FdcId { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("foodCategory")]
    public JsonElement? FoodCategory { get; set; }

    [JsonPropertyName("brandName")]
    public string? BrandName { get; set; }

    [JsonPropertyName("foodNutrients")]
    public List<UsdaFoodNutrient>? FoodNutrients { get; set; } = new();
}

public class UsdaFoodNutrient
{
    // Search response shape
    [JsonPropertyName("nutrientName")]
    public string? NutrientName { get; set; }

    [JsonPropertyName("unitName")]
    public string? UnitName { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    // Detail response shape
    [JsonPropertyName("amount")]
    public double? Amount { get; set; }

    [JsonPropertyName("nutrient")]
    public UsdaNutrientInfo? Nutrient { get; set; }
}

public class UsdaNutrientInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("unitName")]
    public string? UnitName { get; set; }
}