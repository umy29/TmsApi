namespace TmsApi.Application.DTOs;

// Module 6 - Session 1 - Exercise 2: DTOs and Input Validation
// Immutable value container for the wire — deliberately excludes
// Enrollments (the navigation property) and any internal-only fields.
// EnrollmentCount is computed at the query boundary instead.
public record CourseResponseDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollmentCount);

// Module 7 - Session 4 - Exercise 7: whitelist for data shaping.
// nameof() makes this refactor-safe: rename a property and the compiler
// tells you to update this set.
public static class CourseResponseDtoFields
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(CourseResponseDto.Id),
        nameof(CourseResponseDto.Code),
        nameof(CourseResponseDto.Title),
        nameof(CourseResponseDto.MaxCapacity),
        nameof(CourseResponseDto.EnrollmentCount)
    };
}
