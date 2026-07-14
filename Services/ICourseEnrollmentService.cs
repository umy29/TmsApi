using TmsApi.Dtos;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 3: enrollment service for the REST API.
// (updated in Session 3 - Exercise 5, Step 6: list-by-course method)
public interface ICourseEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
}