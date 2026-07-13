using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// (updated in Exercise 2: project to DTOs at the query layer)
public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    // AsNoTracking(): read paths never need change tracking, saves CPU/memory.
    // Select(...) projection: EF translates c.Enrollments.Count into a SQL
    // COUNT(*) subquery — we never load every enrollment row into memory.
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);

        // Re-query through GetByIdAsync so the response uses the same
        // projection — the null! is safe since we just inserted and saved.
        return (await GetByIdAsync(course.Id, ct))!;
    }
}