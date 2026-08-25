using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Authorization;

// Module 11 - Session 3 - Exercise 5: resource-based authorization handler
public class CourseInstructorHandler
    : AuthorizationHandler<CourseInstructorRequirement, Course>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseInstructorRequirement requirement,
        Course resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = context.User.IsInRole("Admin");
        var isInstructor = context.User.IsInRole("Instructor");

        // Admins can manage any course
        if (isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Instructors can only manage courses where InstructorId matches their UserId
        if (isInstructor && resource.InstructorId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
