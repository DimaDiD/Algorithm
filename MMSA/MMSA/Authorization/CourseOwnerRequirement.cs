using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using MMSA.DAL.Constants;
using MMSA.DAL.Repositories;

namespace MMSA.Authorization;

public sealed class CourseOwnerRequirement : IAuthorizationRequirement
{
    public static readonly CourseOwnerRequirement Instance = new();
}

public sealed class CourseOwnerHandler : AuthorizationHandler<CourseOwnerRequirement, CourseAccess>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseOwnerRequirement requirement,
        CourseAccess course)
    {
        var user = context.User;
        var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (user.IsInRole(Roles.Admin) || (user.IsInRole(Roles.Teacher) && course.IsOwner(userId)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public sealed class CourseEditorRequirement : IAuthorizationRequirement
{
    public static readonly CourseEditorRequirement Instance = new();
}

public sealed class CourseEditorHandler : AuthorizationHandler<CourseEditorRequirement, CourseAccess>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseEditorRequirement requirement,
        CourseAccess course)
    {
        var user = context.User;
        var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (user.IsInRole(Roles.Admin) || (user.IsInRole(Roles.Teacher) && course.IsTeacher(userId)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
