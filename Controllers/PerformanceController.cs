using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

// Module 5 - Session 3 - Exercise 7: Many round-trips vs shaped query
[ApiController]
[Route("api/performance")]
public class PerformanceController(TmsDbContext db) : ControllerBase
{
    // Part A: Intentional N+1 (for learning).
    // Loads all students in 1 query, then queries enrollment count for EACH
    // student inside the loop — producing 1 + N total SQL statements.
    [HttpGet("n-plus-one")]
    public async Task<IActionResult> NPlusOneDemo(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n>>> N+1 DEMO: Starting...");

        var students = await db.Students.AsNoTracking().ToListAsync(cancellationToken);

        var results = new List<object>();
        foreach (var s in students)
        {
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, cancellationToken);

            results.Add(new { s.Name, EnrollmentCount = count });
        }

        Console.WriteLine(">>> N+1 DEMO: Finished.\n");
        return Ok(results);
    }

    // Part B: Fix with shaping — the count is computed inside the Select
    // projection, so EF Core translates the whole thing into ONE SQL statement
    // (a correlated subquery), instead of 1 + N round trips.
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
    // Module 5 - Session 3 - Exercise 9: bulk archive using ExecuteUpdateAsync.
// This produces ONE SQL UPDATE statement, not one per row — set-based, not row-by-row.
[HttpPost("archive-old-enrollments")]
public async Task<IActionResult> ArchiveOldEnrollments([FromQuery] int olderThanDays, CancellationToken cancellationToken)
{
    var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);

    var affectedRows = await db.Enrollments
        .Where(e => e.EnrolledAt < cutoff && !e.IsArchived)
        .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true), cancellationToken);

    return Ok(new { ArchivedCount = affectedRows });
}
}
