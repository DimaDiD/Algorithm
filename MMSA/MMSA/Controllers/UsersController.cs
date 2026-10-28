using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MMSA.Authorization;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using System.Security.Claims;

namespace MMSA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Policies.CanManageUsers)]
public class UsersController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    public UsersController(IUserRoleService userRoleService) {  _userRoleService = userRoleService; }

    [HttpPut(ClaimType.Role)]
    public async Task<IActionResult> ChangeRole(ChangeRoleRequestDTO request)
    {
        var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        
        var errors = await _userRoleService.ChangeRoleAsync(request, currentUserId);
        return errors.Any() ? BadRequest(new { errors }) : NoContent();
    }
}