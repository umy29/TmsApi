using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/grades")]
[ApiVersion("2.0")]
public class GradesController : ControllerBase
{
    public record GradeRequest(int StudentId, int CourseId, decimal Score);
    public record GradeResult(string Id, bool Success);

    [HttpPost]
    public IActionResult PostGrade([FromBody] GradeRequest request)
    {
        if (request.Score < 0 || request.Score > 100)
            return BadRequest(new { detail = "Score must be between 0 and 100." });

        var result = new GradeResult(
            Id: Guid.NewGuid().ToString("N")[..12],
            Success: true);

        return Ok(result);
    }
}
