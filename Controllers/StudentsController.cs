using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(TmsDbContext context) : ControllerBase
{
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var students = await context.Students
            .OrderBy(s => s.Name)              // stable sort BEFORE Skip/Take
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }
    [HttpPut("{id}/gpa")]
public async Task<IActionResult> UpdateGpa(int id, [FromQuery] decimal gpa, CancellationToken cancellationToken)
{
    var student = await context.Students.FindAsync([id], cancellationToken);
    if (student is null) return NotFound();

    student.GPA = gpa;
    await context.SaveChangesAsync(cancellationToken);

    var lastUpdated = context.Entry(student).Property("LastUpdated").CurrentValue;
    return Ok(new { student.Id, student.Name, student.GPA, LastUpdated = lastUpdated });
}
[HttpPost("{id}/concurrency-test")]
public async Task<IActionResult> ConcurrencyTest(int id, [FromServices] IDbContextFactory<TmsDbContext> factory, CancellationToken cancellationToken)
{
    // Two independent contexts = two independent "tabs"/requests
    await using var contextA = await factory.CreateDbContextAsync(cancellationToken);
    await using var contextB = await factory.CreateDbContextAsync(cancellationToken);

    var studentA = await contextA.Students.FindAsync([id], cancellationToken);
    var studentB = await contextB.Students.FindAsync([id], cancellationToken);

    if (studentA is null || studentB is null) return NotFound();

    // Tab A saves first
    studentA.Name = studentA.Name + " (edited by Tab A)";
    await contextA.SaveChangesAsync(cancellationToken);

    try
    {
        // Tab B still holds the OLD xmin from before Tab A's save
        studentB.GPA += 0.01m;
        await contextB.SaveChangesAsync(cancellationToken);

        return Ok("No conflict detected (unexpected)");
    }
    catch (DbUpdateConcurrencyException ex)
    {
        return Conflict(new { Message = "Concurrency conflict detected as expected", Detail = ex.Message });
    }
}

[HttpPost("{id}/soft-delete")]
public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
{
    var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    if (student is null) return NotFound();

    student.IsDeleted = true;
    await context.SaveChangesAsync(cancellationToken);
    return Ok(new { student.Id, student.Name, student.IsDeleted });
}

[HttpGet("normal")]
public async Task<IActionResult> GetNormal(CancellationToken cancellationToken)
{
    // HasQueryFilter automatically excludes IsDeleted students here.
    var students = await context.Students.Select(s => s.Name).ToListAsync(cancellationToken);
    return Ok(students);
}

[HttpGet("admin-all")]
public async Task<IActionResult> GetAllIncludingDeleted(CancellationToken cancellationToken)
{
    // IgnoreQueryFilters() bypasses the soft-delete filter for admin/restore scenarios.
    var students = await context.Students
        .IgnoreQueryFilters()
        .Select(s => new { s.Name, s.IsDeleted })
        .ToListAsync(cancellationToken);
    return Ok(students);
}


}