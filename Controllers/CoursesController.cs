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
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        // Binding to CreateCourseRequest triggers automatic model validation
        // against the Data Annotations, via [ApiController] on this class.
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}