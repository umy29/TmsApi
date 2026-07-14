using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// (updated in Session 3 - Exercise 5: HATEOAS links via LinkGenerator)
[ApiController]
[Route("api/courses")]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null) return NotFound();

        // Module 6 - Session 3 - Exercise 5: build hrefs via LinkGenerator,
        // never by string interpolation — links are derived from the same
        // routing metadata the controller already uses, so a future route
        // rename doesn't silently break the API's own self-links.
        var selfHref = linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
        var enrollmentsHref = linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id });

        var links = new List<LinkDto>
        {
            new(selfHref!, "self", "GET"),
            new(selfHref!, "update", "PUT"),
            new(selfHref!, "delete", "DELETE"),
            new(enrollmentsHref!, "enrollments", "GET"),
        };

        // The conditional link is what makes HATEOAS earn its cost — the
        // Angular team uses its presence/absence to render or hide the
        // Enrol button without duplicating the capacity rule in TypeScript.
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(enrollmentsHref!, "enroll", "POST"));
        }

        var detail = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detail);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}