namespace TmsApi.Dtos;

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