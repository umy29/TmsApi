using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/assessments")]
public class AssessmentsController : ControllerBase
{
    [HttpGet("results")]
    [Authorize]
    public IActionResult GetResults()
    {
        return Ok(new { message = "Assessment results" });
    }
}