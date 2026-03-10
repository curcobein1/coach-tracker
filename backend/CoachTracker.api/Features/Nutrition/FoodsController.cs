using Microsoft.AspNetCore.Mvc;

namespace CoachTracker.Api.Features.Nutrition;

[ApiController]
[Route("api/[controller]")]
public class FoodsController : ControllerBase
{
    private readonly UsdaFoodService _usdaFoodService;

    public FoodsController(UsdaFoodService usdaFoodService)
    {
        _usdaFoodService = usdaFoodService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] FoodSearchRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Query)){

        return BadRequest(new {message= "Query is required."});
        }

        var result = await _usdaFoodService.SearchFoodsAsync(request.Query, request.PageSize);
        return Ok(result);
    }

    [HttpGet("{fdcId:int}")]
    public async Task<IActionResult> GetById(int fdcId)
    {
        var result = await _usdaFoodService.GetFoodDetailAsync(fdcId);

        if( result == null)
        {
            return NotFound(new {message ="Food details were not found for this USDA item."});
        }
        return Ok(result);
    }
}
