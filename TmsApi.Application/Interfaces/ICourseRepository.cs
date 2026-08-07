using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

// Module 7 - Session 1 - Exercise 2: repository contract for EnrollStudentHandler.
public interface ICourseRepository
{
    // Must include Enrollments so the handler can check course.Enrollments.Count
    // against course.MaxCapacity without a second round trip.
    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);

    // Module 7 - Session 2 - Exercise 3: needed by CachedCourseService.
    Task<List<Course>> GetAllAsync(CancellationToken ct);
}
