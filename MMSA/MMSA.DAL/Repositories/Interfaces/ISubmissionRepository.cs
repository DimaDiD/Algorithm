using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface ISubmissionRepository : IBaseRepository<Submission>
{
    Task<List<Submission>> GetForExerciseWithAnswersAsync(int exerciseId);
    Task<List<Submission>> GetForClientAsync(Guid clientId, IReadOnlyCollection<int> exerciseIds);
}
