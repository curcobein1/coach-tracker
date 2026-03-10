using Microsoft.AspNetCore.Mvc;

namespace CoachTracker.Api.Features.Plans;

[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    [HttpGet("active")]
    public IActionResult GetActive()
    {
        return Ok(new
        {
            id = 1,
            name = "Default Plan",
            days = Array.Empty<object>()
        });
    }
}