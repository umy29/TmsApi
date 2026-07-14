using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// (updated in Exercise 2: accept/return DTOs, not raw entities)
[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    // Module 6 - Session 2 - Exercise 4, Part B: paginated collection endpoint.
    // [FromQuery] binds from query-string params — without it, ASP.NET Core
    // would try to bind from the body, which is wrong for a GET.
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
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        // Module 6 - Session 1 - Exercise 3: check the business rule BEFORE
        // hitting the database — a duplicate code is a known, expected failure,
        // not a 500. The framework's ProblemDetails middleware handles truly
        // unhandled exceptions; this is a deliberate pre-check, not a try/catch.
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