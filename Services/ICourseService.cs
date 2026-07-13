using TmsApi.Dtos;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// (updated in Exercise 2: return/accept DTOs instead of raw entities)
public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
}