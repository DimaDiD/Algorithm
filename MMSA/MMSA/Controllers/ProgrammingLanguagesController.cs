using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Dtos;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Constants;

namespace MMSA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Policies.CanEditContent)]
public class ProgrammingLanguagesController : CourseEditingControllerBase
{
    private readonly IProgrammingLanguageService _languageService;

    public ProgrammingLanguagesController(IProgrammingLanguageService languageService)
    {
        _languageService = languageService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _languageService.GetAllAsync());
    }

    [HttpPost]
    public Task<IActionResult> Create(ProgrammingLanguageDTO language) =>
        Handle(async () => Ok(await _languageService.CreateAsync(language)));

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public Task<IActionResult> Update(int id, ProgrammingLanguageDTO language) =>
        Handle(async () =>
        {
            language.Id = id;
            await _languageService.UpdateAsync(language);
            return NoContent();
        });
}
