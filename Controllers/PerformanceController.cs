using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/performance")]
public class PerformanceController(TmsDbContext db) : ControllerBase
{
    [HttpGet("n-plus-one")]
    public async Task<IActionResult> NPlusOneDemo(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n>>> N+1 DEMO: Starting...");

        var students = await db.Students.AsNoTracking().ToListAsync(cancellationToken);

        var results = new List<object>();
        foreach (var s in students)
        {
            // Query enrollment count for THIS student, inside the loop.
            // This produces 1 (for students) + N (one per student) SQL statements.
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, cancellationToken);

            results.Add(new { s.Name, EnrollmentCount = count });
        }

        Console.WriteLine(">>> N+1 DEMO: Finished.\n");
        return Ok(results);
    }

    [HttpGet("shaped-query")]
    public async Task<IActionResult> ShapedQueryDemo(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n>>> SHAPED QUERY DEMO: Starting...");

        var report = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        Console.WriteLine(">>> SHAPED QUERY DEMO: Finished.\n");
        return Ok(report);
    }
}