using NSubstitute;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Common;
using TmsApi.Domain.Entities;

namespace TmsApi.Tests;

public class MaxEnrollmentsPerStudentTests
{
    private const int MaxEnrollments = 5;

    private static (IEnrollmentRepository enrollmentRepo, ICourseRepository courseRepo, EnrollStudentHandler handler)
        BuildHandler(int existingEnrollmentCount, bool courseExists = true, bool alreadyEnrolled = false)
    {
        var enrollmentRepo = Substitute.For<IEnrollmentRepository>();
        var courseRepo = Substitute.For<ICourseRepository>();

        if (courseExists)
        {
            var course = new Course
            {
                Id = 1,
                Code = "CS-500",
                Title = "Advanced Topics",
                MaxCapacity = 100,
                Enrollments = new List<Enrollment>(),
            };
            courseRepo.GetByCodeAsync("CS-500", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Course?>(course));
        }

        enrollmentRepo
            .ExistsAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(alreadyEnrolled));

        enrollmentRepo
            .CountByStudentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(existingEnrollmentCount));

        var handler = new EnrollStudentHandler(enrollmentRepo, courseRepo);
        return (enrollmentRepo, courseRepo, handler);
    }

    [Fact]
    public async Task Handle_WhenStudentHasMaxEnrollments_ReturnsLimitError()
    {
        var (_, _, handler) = BuildHandler(existingEnrollmentCount: MaxEnrollments);
        var command = new EnrollStudentCommand(StudentId: 1, CourseCode: "CS-500");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("enrollment_limit_reached", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenStudentHasFourEnrollments_CanEnroll()
    {
        var (enrollmentRepo, _, handler) = BuildHandler(existingEnrollmentCount: MaxEnrollments - 1);
        var command = new EnrollStudentCommand(StudentId: 1, CourseCode: "CS-500");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await enrollmentRepo.Received(1).AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStudentHasZeroEnrollments_CanEnroll()
    {
        var (enrollmentRepo, _, handler) = BuildHandler(existingEnrollmentCount: 0);
        var command = new EnrollStudentCommand(StudentId: 1, CourseCode: "CS-500");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await enrollmentRepo.Received(1).AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }
}
