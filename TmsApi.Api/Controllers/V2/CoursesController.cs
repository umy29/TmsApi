using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V2;

// Module 7 - Session 1 - Exercise 1, Step 3: V2 course listing.
// V2 wraps the same data in a { data, meta, links } envelope per the
// Module 7 spine — paging fields move to meta, discoverability hints move to links.
[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);

        var hasNext = result.HasNext;
        var hasPrevious = result.HasPrevious;

        return Ok(new
        {
            data = result.Items,
            meta = new
            {
                result.TotalCount,
                result.Page,
                result.PageSize,
                result.TotalPages,
                hasNext,
                hasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={result.Page}&pageSize={result.PageSize}",
                next = hasNext
                    ? $"/api/v2/courses?page={result.Page + 1}&pageSize={result.PageSize}"
                    : (string?)null,
                prev = hasPrevious
                    ? $"/api/v2/courses?page={result.Page - 1}&pageSize={result.PageSize}"
                    : (string?)null,
                enroll = "/api/v2/enrollments"
            }
        });
    }
}
