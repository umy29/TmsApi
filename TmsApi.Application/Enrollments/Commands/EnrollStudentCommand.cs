using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Enrollments.Commands;

// Module 7 - Session 1 - Exercise 2, Step 2: the command's return type
// IRequest<Result<EnrollmentCreated, EnrollmentError>> is the contract
// MediatR uses to find the right handler. The controller never sees this
// type directly — only IMediator.Send(command) and the unwrapped result.
public record EnrollStudentCommand(int StudentId, string CourseCode)
    : IRequest<Result<EnrollmentCreated, EnrollmentError>>;

public record EnrollmentCreated(int EnrollmentId, int StudentId, string CourseCode);
