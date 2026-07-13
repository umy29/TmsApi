namespace TmsApi.Dtos;

// Module 6 - Session 1 - Exercise 3: enrollment response DTO.
public record EnrollmentResponseDto(
    int Id,
    int CourseId,
    int StudentId,
    DateTime EnrolledAt);