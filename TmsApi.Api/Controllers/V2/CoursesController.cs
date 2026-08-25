using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Api.Authorization;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Utilities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(
    ICachedCourseService cachedCourseService,
    TmsDbContext dbContext,
    IAuthorizationService authorizationService) : ControllerBase
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
        var rows = allCourses.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var hasNext = page < totalPages;
        var hasPrevious = page > 1;
        var shaped = rows.ShapeData(fields, CourseResponseDtoFields.Allowed);
        var links = new List<LinkDto>
        {
            new($"/api/v2/courses?page={page}&pageSize={pageSize}{(fields != null ? $"&fields={fields}" : "")}", "self", "GET")
        };
        if (hasNext)
            links.Add(new($"/api/v2/courses?page={page + 1}&pageSize={pageSize}{(fields != null ? $"&fields={fields}" : "")}", "next", "GET"));
        if (hasPrevious)
            links.Add(new($"/api/v2/courses?page={page - 1}&pageSize={pageSize}{(fields != null ? $"&fields={fields}" : "")}", "prev", "GET"));
        return Ok(new { data = shaped, meta = new { totalCount, page, pageSize, totalPages, hasNext, hasPrevious }, links });
    }

    public record UpdateCourseDto(string Title);

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
    {
        var course = await dbContext.Courses.FindAsync(new object[] { id }, ct);
        if (course is null) return NotFound();

        var authResult = await authorizationService.AuthorizeAsync(User, course, "CanEditCourse");
        if (!authResult.Succeeded)
            return Forbid();

        course.Title = dto.Title;
        await dbContext.SaveChangesAsync(ct);
        await cachedCourseService.InvalidateCourseCacheAsync(ct);
        return NoContent();
    }
}