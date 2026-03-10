using Microsoft.AspNetCore.Mvc;

namespace CoachTracker.Api.Features.Settings;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            unitSystem = "metric",
            theme = "dark"
        });
    }
}