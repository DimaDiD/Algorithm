using MMSA.BLL.Dtos;
using MMSA.BLL.Dtos.Exercises;

namespace MMSA.BLL.Services.Interfaces;

public interface IExerciseService
{
    Task<List<ExerciseDTO>> GetForTopicAsync(int pageId, int? subPageId);
    Task<ExerciseEditDTO?> GetForEditAsync(int id);
    Task<int> CreateAsync(ExerciseEditDTO exercise);
    Task UpdateAsync(ExerciseEditDTO exercise);
    Task DeleteAsync(int id);
    Task MoveAsync(int id, MoveDirection direction);
    Task<int> DeleteClientDataAsync(Guid clientId);
    Task<ExerciseCheckResultDTO> CheckAsync(
        int id,
        ExerciseCheckRequestDTO request,
        ExerciseCheckContext context,
        CancellationToken cancellationToken);
    AiStatusDTO GetAiStatus();
    Task<List<ExerciseProgressDTO>> GetProgressAsync(int pageId, int? subPageId, Guid clientId);
    Task<ExerciseStatsDTO?> GetStatsAsync(int id);
}
