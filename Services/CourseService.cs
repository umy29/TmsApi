using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// Primary-constructor DI, ILogger at the service boundary (not the controller),
// and CancellationToken on every async method — production habits from day one.
public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Course> CreateAsync(Course course, CancellationToken ct)
    {
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);
        return course;
    }
}