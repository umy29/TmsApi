using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// (updated in Exercise 2: DTOs; Exercise 3: duplicate-code check;
// Session 2 Exercise 4: paginated collection query)
public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
}