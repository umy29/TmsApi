using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;
using TmsApi.Services;

namespace TmsApi.Controllers;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// [Route("api/courses")]: plural noun, no verbs — resource-named routes.
[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    // {id:int} route constraint: a bad ID like /api/courses/abc returns 404
    // at the routing layer, never reaching this action — fail fast.
    // Name = nameof(GetCourseById) names the route so CreatedAtAction (and
    // later, LinkGenerator in Exercise 5) can reference it reliably.
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
    {
        var result = await courseService.CreateAsync(course, ct);
        // CreatedAtAction sets the Location header automatically, pointing
        // back at GetCourseById with the new resource's id.
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}