using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

// Module 5 - Session 1 - Exercise 2, Steps 3-4: Deferred Execution and Translation Failure experiments
[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        // Building the query does NOT touch the database yet —
        // Where/OrderBy just build up an expression tree (IQueryable).
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);

        // ToList() is what actually triggers SQL execution (materialization).
        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList();

        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    // Plain C# method — EF Core cannot translate this into SQL.
    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            // EF Core tries to convert this LINQ expression into a SQL AST,
            // but IsHonorRoll is compiled IL, not something Npgsql can translate.
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }
}