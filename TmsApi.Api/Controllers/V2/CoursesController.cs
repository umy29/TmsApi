using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Utilities;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(ICachedCourseService cachedCourseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] string? fields,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var allCourses = await cachedCourseService.GetAllCoursesAsync(ct);
        var totalCount = allCourses.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var rows = allCourses
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        // ShapeData throws BadRequestException on unknown fields —
        // GlobalExceptionHandler maps that to 400 application/problem+json.
        var shaped = rows.ShapeData(fields, CourseResponseDtoFields.Allowed);

        var links = new List<LinkDto>
        {
            new($"/api/v2/courses?page={page}&pageSize={pageSize}{(fields != null ? $"&fields={fields}" : "")}", "self", "GET")
        };
        if (hasNext)
            links.Add(new($"/api/v2/courses?page={page + 1}&pageSize={pageSize}{(fields != null ? $"&fields={fields}" : "")}", "next", "GET"));
        if (hasPrevious)
            links.Add(new($"/api/v2/courses?page={page - 1}&pageSize={pageSize}{(fields != null ? $"&fields={fields}" : "")}", "prev", "GET"));

        return Ok(new
        {
            data = shaped,
            meta = new { totalCount, page, pageSize, totalPages, hasNext, hasPrevious },
            links
        });
    }
}
