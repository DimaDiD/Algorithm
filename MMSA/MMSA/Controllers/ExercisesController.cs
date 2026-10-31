using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Dtos.Exercises;
using Microsoft.AspNetCore.RateLimiting;
using MMSA.BLL.Services.Interfaces;
using MMSA.RateLimiting;

namespace MMSA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Policies.CanEditContent)]
public class ExercisesController : CourseEditingControllerBase
{
    private readonly IExerciseService _exerciseService;
    private readonly ICourseService _courseService;

    public ExercisesController(IExerciseService exerciseService, ICourseService courseService)
    {
        _exerciseService = exerciseService;
        _courseService = courseService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetForTopic(int pageId, int? subPageId)
    {
        return Ok(await _exerciseService.GetForTopicAsync(pageId, subPageId));
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.ExerciseCheck)]
    [HttpPost("{id:int}/check")]
    public Task<IActionResult> Check(int id, ExerciseCheckRequestDTO request) =>
        Handle(async () =>
        {
            var isCourseEditor = await CanEditAsync(await _courseService.GetAccessForExerciseAsync(id));
            return Ok(await _exerciseService.CheckAsync(id, request, recordSubmission: !isCourseEditor));
        });

    [AllowAnonymous]
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress(int pageId, int? subPageId, Guid clientId)
    {
        return Ok(await _exerciseService.GetProgressAsync(pageId, subPageId, clientId));
    }

    [HttpGet("{id:int}/stats")]
    public Task<IActionResult> GetStats(int id) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForExerciseAsync(id)) is { } denied)
                return denied;

            var stats = await _exerciseService.GetStatsAsync(id);
            return stats is null ? NotFound() : Ok(stats);
        });

    [HttpGet("{id:int}/edit")]
    public Task<IActionResult> GetForEdit(int id) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForExerciseAsync(id)) is { } denied)
                return denied;

            var exercise = await _exerciseService.GetForEditAsync(id);
            return exercise is null ? NotFound() : Ok(exercise);
        });

    [HttpPost]
    public Task<IActionResult> Create(ExerciseEditDTO exercise) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForTopicAsync(exercise.PageId, exercise.SubPageId)) is { } denied)
                return denied;

            return Ok(new { id = await _exerciseService.CreateAsync(exercise) });
        });

    [HttpPut("{id:int}")]
    public Task<IActionResult> Update(int id, ExerciseEditDTO exercise) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForExerciseAsync(id)) is { } denied)
                return denied;

            exercise.Id = id;
            await _exerciseService.UpdateAsync(exercise);
            return NoContent();
        });

    [HttpDelete("{id:int}")]
    public Task<IActionResult> Delete(int id) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForExerciseAsync(id)) is { } denied)
                return denied;

            await _exerciseService.DeleteAsync(id);
            return NoContent();
        });
}
