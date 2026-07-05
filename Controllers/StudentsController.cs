using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

// Module 5 - Session 2 - Exercise 3: GroupBy, aggregates, and pagination
[ApiController]
[Route("api/students")]
public class StudentsController(TmsDbContext context) : ControllerBase
{
    // Task 1: Paged list of students, page size configurable, stable sort by name.
    // IMPORTANT: OrderBy must come BEFORE Skip/Take, or PostgreSQL may return
    // rows in an unpredictable order between pages.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var students = await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    // Module 5 - Session 2 - Exercise 3, Task 2: top 5 courses by enrollment count.
// Same shape as Session 1's top-courses query, capped with Take(5).
[HttpGet("top5-courses-by-enrollment")]
public async Task<IActionResult> Top5CoursesByEnrollment(CancellationToken cancellationToken)
{
    var list = await context.Courses
        .Select(c => new
        {
            c.Title,
            EnrollmentCount = c.Enrollments.Count
        })
        .OrderByDescending(x => x.EnrollmentCount)
        .Take(5)
        .ToListAsync(cancellationToken);

    return Ok(list);
}
}