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
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
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