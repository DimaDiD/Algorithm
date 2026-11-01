using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Dtos;
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
    public Task<IActionResult> GetForTopic(int pageId, int? subPageId) =>
        Handle(async () =>
        {
            if (await EnsureCanReadAsync(_courseService.GetAccessForTopicAsync(pageId, subPageId)) is { } denied)
                return denied;

            return Ok(await _exerciseService.GetForTopicAsync(pageId, subPageId));
        });

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.ExerciseCheck)]
    [HttpPost("{id:int}/check")]
    public Task<IActionResult> Check(int id, ExerciseCheckRequestDTO request) =>
        Handle(async () =>
        {
            var access = await _courseService.GetAccessForExerciseAsync(id);
            var isCourseEditor = await CanEditAsync(access);
            if (access is null || (!access.IsPublished && !isCourseEditor))
                return NotFound(new { errors = new[] { "Завдання не знайдено." } });

            var context = new ExerciseCheckContext(
                RecordSubmission: !isCourseEditor,
                ClientKey: RateLimitPolicies.GetClientKey(HttpContext));

            return Ok(await _exerciseService.CheckAsync(id, request, context, HttpContext.RequestAborted));
        });

    [AllowAnonymous]
    [HttpGet("ai-status")]
    public IActionResult GetAiStatus()
    {
        return Ok(_exerciseService.GetAiStatus());
    }

    [AllowAnonymous]
    [HttpGet("progress")]
    public Task<IActionResult> GetProgress(int pageId, int? subPageId, Guid clientId) =>
        Handle(async () =>
        {
            if (await EnsureCanReadAsync(_courseService.GetAccessForTopicAsync(pageId, subPageId)) is { } denied)
                return denied;

            return Ok(await _exerciseService.GetProgressAsync(pageId, subPageId, clientId));
        });

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.ExerciseCheck)]
    [HttpDelete("my-data")]
    public Task<IActionResult> DeleteMyData(Guid clientId) =>
        Handle(async () => Ok(new { deleted = await _exerciseService.DeleteClientDataAsync(clientId) }));

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

    [HttpPut("{id:int}/move")]
    public Task<IActionResult> Move(int id, MoveDirection direction) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForExerciseAsync(id)) is { } denied)
                return denied;

            await _exerciseService.MoveAsync(id, direction);
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
