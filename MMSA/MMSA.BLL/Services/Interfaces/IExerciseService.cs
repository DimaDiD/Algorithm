using MMSA.BLL.Dtos.Exercises;

namespace MMSA.BLL.Services.Interfaces;

public interface IExerciseService
{
    Task<List<ExerciseDTO>> GetForTopicAsync(int pageId, int? subPageId);
    Task<ExerciseEditDTO?> GetForEditAsync(int id);
    Task<int> CreateAsync(ExerciseEditDTO exercise);
    Task UpdateAsync(ExerciseEditDTO exercise);
    Task DeleteAsync(int id);
    Task<ExerciseCheckResultDTO> CheckAsync(int id, ExerciseCheckRequestDTO request, bool recordSubmission);
    Task<List<ExerciseProgressDTO>> GetProgressAsync(int pageId, int? subPageId, Guid clientId);
    Task<ExerciseStatsDTO?> GetStatsAsync(int id);
}
