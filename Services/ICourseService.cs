using TmsApi.Entities;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// Service layer between the controller and persistence — keeps business
// rules and side effects out of the controller, testable in isolation.
public interface ICourseService
{
    Task<Course?> GetByIdAsync(int id, CancellationToken ct);
    Task<Course> CreateAsync(Course course, CancellationToken ct);
}