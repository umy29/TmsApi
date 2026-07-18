namespace TmsApi.Application.DTOs;

// Module 6 - Session 3 - Exercise 5: detail shape for GET /api/courses/{id}.
// Everything in CourseResponseDto plus the Links array. The list/page
// response keeps using CourseResponseDto — no per-item links there.
public record CourseDetailDto
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required int MaxCapacity { get; init; }
    public required int EnrollmentCount { get; init; }
    public required IReadOnlyList<LinkDto> Links { get; init; }
}