using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MMSA.Authorization;
using MMSA.BLL.Dtos.Courses;
using MMSA.DAL.Constants;
using MMSA.DAL.Repositories;

namespace MMSA.Controllers;

public abstract class CourseEditingControllerBase : ControllerBase
{
    protected CurrentUser CurrentUser =>
        new(User.FindFirstValue(JwtRegisteredClaimNames.Sub), User.IsInRole(Roles.Admin), User.IsInRole(Roles.Teacher));

    protected async Task<bool> CanEditAsync(CourseAccess? access)
    {
        if (access is null || User.Identity?.IsAuthenticated != true)
            return false;

        return await AuthorizeAsync(access, CourseEditorRequirement.Instance);
    }

    protected async Task<IActionResult?> EnsureCanReadAsync(Task<CourseAccess?> accessTask)
    {
        var access = await accessTask;
        if (access is not null && (access.IsPublished || await CanEditAsync(access)))
            return null;

        return NotFound(new { errors = new[] { "Об'єкт не знайдено." } });
    }

    protected Task<IActionResult?> EnsureCanEditAsync(Task<CourseAccess?> accessTask) =>
        EnsureAsync(accessTask, CourseEditorRequirement.Instance);

    protected Task<IActionResult?> EnsureCanManageAsync(Task<CourseAccess?> accessTask) =>
        EnsureAsync(accessTask, CourseOwnerRequirement.Instance);

    private async Task<IActionResult?> EnsureAsync(Task<CourseAccess?> accessTask, IAuthorizationRequirement requirement)
    {
        var access = await accessTask;
        if (access is null)
            return NotFound(new { errors = new[] { "Об'єкт не знайдено." } });

        return await AuthorizeAsync(access, requirement) ? null : Forbid();
    }

    private async Task<bool> AuthorizeAsync(CourseAccess access, IAuthorizationRequirement requirement)
    {
        var authorizationService = HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        return (await authorizationService.AuthorizeAsync(User, access, requirement)).Succeeded;
    }

    protected async Task<IActionResult> Handle(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { errors = new[] { exception.Message } });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { errors = new[] { exception.Message } });
        }
    }
}
