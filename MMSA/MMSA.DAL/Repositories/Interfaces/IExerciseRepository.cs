using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface IExerciseRepository : IBaseRepository<Exercise>
{
    Task<List<Exercise>> GetByTopicAsync(int pageId, int? subPageId);
    Task<Exercise?> GetWithQuestionsAsync(int id, bool track = false);
    Task<int> GetNextOrderAsync(int pageId, int? subPageId);
}
