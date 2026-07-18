using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

// Module 6 - Session 1 - Exercise 3: enrollment request DTO with validation.
public record EnrollStudentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    public required int StudentId { get; init; }
}