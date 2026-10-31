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
        new(User.FindFirstValue(JwtRegisteredClaimNames.Sub), User.IsInRole(Roles.Admin));

    protected async Task<bool> CanEditAsync(CourseAccess? access)
    {
        if (access is null || User.Identity?.IsAuthenticated != true)
            return false;

        var authorizationService = HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        return (await authorizationService.AuthorizeAsync(User, access, CourseOwnerRequirement.Instance)).Succeeded;
    }

    protected async Task<IActionResult?> EnsureCanEditAsync(Task<CourseAccess?> accessTask)
    {
        var access = await accessTask;
        if (access is null)
            return NotFound(new { errors = new[] { "Об'єкт не знайдено." } });

        var authorizationService = HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        var result = await authorizationService.AuthorizeAsync(User, access, CourseOwnerRequirement.Instance);

        return result.Succeeded ? null : Forbid();
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
