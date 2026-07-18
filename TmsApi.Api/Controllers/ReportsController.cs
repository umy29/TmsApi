using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

// Module 5 - Session 1 - Exercise 2, Step 5: Solve the Registrar's Business Queries
// Each endpoint below is verified against the SQL log to confirm filtering,
// sorting, and aggregation happen in the database — not pulled into C# first.
[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
    // Query 1: How many active students have GPA >= 3.0?
    // Expect SQL: SELECT COUNT(*) ... WHERE "IsActive" AND "GPA" >= 3.0
    [HttpGet("active-honor-count")]
    public async Task<IActionResult> ActiveHonorCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new { count });
    }

    // Query 2: Which courses have the most enrollments, sorted descending?
    // Expect SQL: correlated subquery COUNT + ORDER BY ... DESC
    [HttpGet("top-courses-by-enrollment")]
    public async Task<IActionResult> TopCoursesByEnrollment()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    // Query 3: What is the average GPA per course?
    // Expect SQL: GROUP BY with AVG aggregate
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> AverageGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    // Query 4a: Which students have zero enrollments? (subquery approach)
    // Expect SQL: WHERE NOT EXISTS (...)
    [HttpGet("zero-enrollment-students-subquery")]
    public async Task<IActionResult> ZeroEnrollmentStudentsSubquery()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // Query 4b: Same result, using EF Core 10's LeftJoin extension.
    // Expect SQL: LEFT JOIN ... WHERE ... IS NULL
    [HttpGet("zero-enrollment-students-leftjoin")]
    public async Task<IActionResult> ZeroEnrollmentStudentsLeftJoin()
    {
        var list = await context.Students
            .LeftJoin(context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();

        return Ok(list);
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