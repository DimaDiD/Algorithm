using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using System.Security.Claims;

namespace MMSA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    private string CurrentUserId => User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await _profileService.GetAsync(CurrentUserId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileRequestDTO request)
    {
        var errors = await _profileService.UpdateAsync(CurrentUserId, request);
        return errors.Count == 0 ? NoContent() : BadRequest(new { errors });
    }
}
