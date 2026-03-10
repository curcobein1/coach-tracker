using System.Text.Json;

namespace CoachTracker.Api.Features.Nutrition;

public class UsdaFoodService
{

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public UsdaFoodService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    // SEARCH FOODS
    public async Task<FoodSearchResponseDto> SearchFoodsAsync(string query, int pageSize)
    {
        var apiKey = _configuration["Usda:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("USDA API key missing");

        var payload = new
        {
            query,
            pageSize
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://api.nal.usda.gov/fdc/v1/foods/search?api_key={apiKey}",
            payload);

        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var usda = JsonSerializer.Deserialize<UsdaSearchResponse>(raw, options)
                   ?? new UsdaSearchResponse();

        return new FoodSearchResponseDto
        {
            Query = query,
            PageSize = pageSize,
            Foods = usda.Foods.Select(f => new FoodSearchItemDto
            {
                FdcId = f.FdcId,
                Description = f.Description,
                FoodCategory = GetFoodCategoryText(f.FoodCategory),
                BrandName = f.BrandName
            }).ToList()
        };
    }

    // FOOD DETAIL
    public async Task<FoodDetailDto> GetFoodDetailAsync(int fdcId)
    {
        var apiKey = _configuration["Usda:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("USDA API key missing");

        var response = await _httpClient.GetAsync(
            $"https://api.nal.usda.gov/fdc/v1/food/{fdcId}?api_key={apiKey}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var food = JsonSerializer.Deserialize<UsdaFoodItem>(raw, options)
                   ?? throw new Exception("Food not found");

        return MapFoodDetail(food);
    }

    private static string? GetFoodCategoryText(JsonElement? foodCategory)
    {
        if (foodCategory is null)
            return null;

        var value = foodCategory.Value;

        if (value.ValueKind == JsonValueKind.String)
            return value.GetString();

        if (value.ValueKind == JsonValueKind.Object)
        {
            if (value.TryGetProperty("description", out var description))
                return description.GetString();

            if (value.TryGetProperty("name", out var name))
                return name.GetString();
        }

        return null;
    }

    // MAP USDA FOOD → DETAIL DTO
    private static FoodDetailDto MapFoodDetail(UsdaFoodItem food)
    {
        var dto = new FoodDetailDto
        {
            FdcId = food.FdcId,
            Description = food.Description,
            FoodCategory = GetFoodCategoryText(food.FoodCategory)
        };

        dto.Summary = new FoodSummaryDto
        {
            Calories = GetNutrientValue(food, "Energy"),
            Protein = GetNutrientValue(food, "Protein"),
            Carbs = GetNutrientValue(food, "Carbohydrate, by difference"),
            Fat = GetNutrientValue(food, "Total lipid (fat)"),
            Fiber = GetNutrientValue(food, "Fiber, total dietary"),
            Sugar = GetNutrientValue(food, "Total Sugars"),
            Sodium = GetNutrientValue(food, "Sodium, Na")
        };

        foreach (var nutrient in food.FoodNutrients ?? new())
        {
            var nutrientName = nutrient.NutrientName ?? nutrient.Nutrient?.Name;
            var nutrientValue = nutrient.Value ?? nutrient.Amount;

            if (string.IsNullOrWhiteSpace(nutrientName) || nutrientValue == null)
                continue;

            if (NutrientMapper.EssentialMicronutrients.TryGetValue(nutrientName, out var map))
            {
                var item = new NutrientItemDto
                {
                    Key = map.Key,
                    Label = map.Label,
                    Unit = map.Unit,
                    Value = nutrientValue
                };

                switch (map.Group)
                {
                    case "vitamins":
                        dto.Micronutrients.Vitamins.Add(item);
                        break;

                    case "majorMinerals":
                        dto.Micronutrients.MajorMinerals.Add(item);
                        break;

                    case "traceMinerals":
                        dto.Micronutrients.TraceMinerals.Add(item);
                        break;
                }
            }
        }

        return dto;
    }

    // HELPER
    private static double? GetNutrientValue(UsdaFoodItem food, string nutrientName)
    {
        var nutrient = food.FoodNutrients?.FirstOrDefault(n =>
            string.Equals(n.NutrientName, nutrientName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(n.Nutrient?.Name, nutrientName, StringComparison.OrdinalIgnoreCase));

        return nutrient?.Value ?? nutrient?.Amount;
    }
}

public static class NutrientMapper
{
    public static readonly Dictionary<string, NutrientMapItem> EssentialMicronutrients =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Vitamins
            ["Vitamin A, RAE"] = new("vitamin_a", "Vitamin A", "vitamins", "mcg"),
            ["Vitamin C, total ascorbic acid"] = new("vitamin_c", "Vitamin C", "vitamins", "mg"),
            ["Vitamin D (D2 + D3)"] = new("vitamin_d", "Vitamin D", "vitamins", "mcg"),
            ["Vitamin E (alpha-tocopherol)"] = new("vitamin_e", "Vitamin E", "vitamins", "mg"),
            ["Vitamin K (phylloquinone)"] = new("vitamin_k", "Vitamin K", "vitamins", "mcg"),
            ["Thiamin"] = new("thiamin", "Thiamin (B1)", "vitamins", "mg"),
            ["Riboflavin"] = new("riboflavin", "Riboflavin (B2)", "vitamins", "mg"),
            ["Niacin"] = new("niacin", "Niacin (B3)", "vitamins", "mg"),
            ["Pantothenic acid"] = new("pantothenic_acid", "Pantothenic Acid (B5)", "vitamins", "mg"),
            ["Vitamin B-6"] = new("vitamin_b6", "Vitamin B6", "vitamins", "mg"),
            ["Biotin"] = new("biotin", "Biotin (B7)", "vitamins", "mcg"),
            ["Folate, DFE"] = new("folate", "Folate (B9)", "vitamins", "mcg"),
            ["Vitamin B-12"] = new("vitamin_b12", "Vitamin B12", "vitamins", "mcg"),

            // Major minerals
            ["Calcium, Ca"] = new("calcium", "Calcium", "majorMinerals", "mg"),
            ["Phosphorus, P"] = new("phosphorus", "Phosphorus", "majorMinerals", "mg"),
            ["Magnesium, Mg"] = new("magnesium", "Magnesium", "majorMinerals", "mg"),
            ["Sodium, Na"] = new("sodium", "Sodium", "majorMinerals", "mg"),
            ["Potassium, K"] = new("potassium", "Potassium", "majorMinerals", "mg"),
            ["Chloride, Cl"] = new("chloride", "Chloride", "majorMinerals", "mg"),

            // Trace minerals
            ["Iron, Fe"] = new("iron", "Iron", "traceMinerals", "mg"),
            ["Zinc, Zn"] = new("zinc", "Zinc", "traceMinerals", "mg"),
            ["Copper, Cu"] = new("copper", "Copper", "traceMinerals", "mg"),
            ["Manganese, Mn"] = new("manganese", "Manganese", "traceMinerals", "mg"),
            ["Iodine, I"] = new("iodine", "Iodine", "traceMinerals", "mcg"),
            ["Selenium, Se"] = new("selenium", "Selenium", "traceMinerals", "mcg"),
            ["Chromium, Cr"] = new("chromium", "Chromium", "traceMinerals", "mcg"),
            ["Molybdenum, Mo"] = new("molybdenum", "Molybdenum", "traceMinerals", "mcg"),
            ["Fluoride, F"] = new("fluoride", "Fluoride", "traceMinerals", "mg"),
        };
}

public record NutrientMapItem(
    string Key,
    string Label,
    string Group,
    string Unit
);