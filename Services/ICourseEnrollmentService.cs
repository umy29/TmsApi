using TmsApi.Dtos;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 3: enrollment service for the REST API.
// Named distinctly from the M4 IEnrollmentService (in-memory worker service)
// to avoid a DI registration collision.
public interface ICourseEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
}