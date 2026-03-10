using Microsoft.AspNetCore.Mvc;

namespace CoachTracker.Api.Features.Workouts;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    [HttpGet("today")]
    public IActionResult GetToday()
    {
        return Ok(new
        {
            date = DateTime.Today.ToString("yyyy-MM-dd"),
            planDayTitle = "Sample Day",
            items = Array.Empty<object>()
        });
    }
}