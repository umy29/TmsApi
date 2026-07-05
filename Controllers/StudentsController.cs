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
// Module 5 - Session 3 - Exercise 8: concurrency conflict test.
// Uses two independent DbContext instances (via the factory) to genuinely
// simulate "two browser tabs" editing the same student at the same time.
[HttpPost("{id}/concurrency-test")]
public async Task<IActionResult> ConcurrencyTest(int id, [FromServices] IDbContextFactory<TmsDbContext> factory, CancellationToken cancellationToken)
{
    await using var contextA = await factory.CreateDbContextAsync(cancellationToken);
    await using var contextB = await factory.CreateDbContextAsync(cancellationToken);

    var studentA = await contextA.Students.FindAsync([id], cancellationToken);
    var studentB = await contextB.Students.FindAsync([id], cancellationToken);

    if (studentA is null || studentB is null) return NotFound();

    // Tab A saves first — its xmin value updates in the database.
    studentA.Name = studentA.Name + " (edited by Tab A)";
    await contextA.SaveChangesAsync(cancellationToken);

    try
    {
        // Tab B still holds the OLD xmin from before Tab A's save,
        // so this UPDATE's WHERE clause matches zero rows -> EF throws.
        studentB.GPA += 0.01m;
        await contextB.SaveChangesAsync(cancellationToken);

        return Ok("No conflict detected (unexpected)");
    }
    catch (DbUpdateConcurrencyException ex)
    {
        return Conflict(new { Message = "Concurrency conflict detected as expected", Detail = ex.Message });
    }
}

// Module 5 - Session 3 - Exercise 9: soft-delete a student.
[HttpPost("{id}/soft-delete")]
public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
{
    var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    if (student is null) return NotFound();

    student.IsDeleted = true;
    await context.SaveChangesAsync(cancellationToken);
    return Ok(new { student.Id, student.Name, student.IsDeleted });
}

// Module 5 - Session 3 - Exercise 9: normal query — HasQueryFilter
// automatically excludes IsDeleted students, no extra code needed here.
[HttpGet("normal")]
public async Task<IActionResult> GetNormal(CancellationToken cancellationToken)
{
    var students = await context.Students.Select(s => s.Name).ToListAsync(cancellationToken);
    return Ok(students);
}

// Module 5 - Session 3 - Exercise 9: admin/restore view —
// IgnoreQueryFilters() bypasses the soft-delete filter.
[HttpGet("admin-all")]
public async Task<IActionResult> GetAllIncludingDeleted(CancellationToken cancellationToken)
{
    var students = await context.Students
        .IgnoreQueryFilters()
        .Select(s => new { s.Name, s.IsDeleted })
        .ToListAsync(cancellationToken);
    return Ok(students);
}
}

