using Microsoft.AspNetCore.Mvc;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "UP" });
}
