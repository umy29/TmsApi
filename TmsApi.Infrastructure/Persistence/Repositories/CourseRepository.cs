using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Repositories;

// Module 7 - Session 1 - Exercise 2: EF-backed course repository.
public class CourseRepository(TmsDbContext context) : ICourseRepository
{
    public Task<Course?> GetByCodeAsync(string code, CancellationToken ct) =>
        context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == code, ct);

    // Module 7 - Session 2 - Exercise 3: fetch all courses for the cached service.
    public Task<List<Course>> GetAllAsync(CancellationToken ct) =>
        context.Courses
            .Include(c => c.Enrollments)
            .OrderBy(c => c.Title)
            .AsNoTracking()
            .ToListAsync(ct);
}
