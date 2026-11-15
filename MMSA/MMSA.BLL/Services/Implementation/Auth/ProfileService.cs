using Microsoft.AspNetCore.Identity;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Implementation.Auth;

public sealed class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ProfileDTO?> GetAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null
            ? null
            : new ProfileDTO
            {
                Email = user.Email ?? string.Empty,
                LastName = user.LastName,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName
            };
    }

    public async Task<IReadOnlyList<string>> UpdateAsync(string userId, UpdateProfileRequestDTO request)
    {
        var lastName = PersonNames.Normalize(request.LastName);
        var firstName = PersonNames.Normalize(request.FirstName);
        var middleName = PersonNames.Normalize(request.MiddleName);

        var errors = PersonNames.Validate(lastName, firstName, middleName);
        if (errors.Count > 0)
            return errors;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new[] { "Користувача не знайдено." };

        user.LastName = lastName;
        user.FirstName = firstName;
        user.MiddleName = middleName;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? Array.Empty<string>() : result.Errors.Select(error => error.Description).ToArray();
    }
}
