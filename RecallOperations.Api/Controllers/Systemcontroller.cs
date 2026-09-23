using Microsoft.AspNetCore.Mvc;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    [HttpGet("welcome")]
    public IActionResult GetWelcome()
    {
        return Ok(new
        {
            message = "Welcome to the Vehicle Recall Operations API"
        });
    }
}