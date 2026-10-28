using Microsoft.AspNetCore.Identity;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Implementation.Auth;

public sealed class UserRoleService : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRoleService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<string>> ChangeRoleAsync(
        ChangeRoleRequestDTO request, string currentUserId)
    {
        if (!Roles.All.Contains(request.Role))
            return new[] { $"Unknown role '{request.Role}'." };

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new[] { "User not found." };

        if (user.Id == currentUserId)
            return new[] { "You cannot change your own role." };

        var currentRoles = await _userManager.GetRolesAsync(user);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return removeResult.Errors.Select(e => e.Description).ToArray();

        var addResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addResult.Succeeded)
            return addResult.Errors.Select(e => e.Description).ToArray();

        await _userManager.UpdateSecurityStampAsync(user);

        return Array.Empty<string>();
    }
}