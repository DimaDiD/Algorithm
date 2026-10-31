using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class SubmissionRepository : BaseRepository<Submission>, ISubmissionRepository
{
    public SubmissionRepository(AlgorithmDataContext dataContext) : base(dataContext)
    {
    }

    public Task<List<Submission>> GetForExerciseWithAnswersAsync(int exerciseId) =>
        _dataContext.Submissions
            .AsNoTracking()
            .Where(submission => submission.ExerciseId == exerciseId)
            .Include(submission => submission.Answers)
            .OrderBy(submission => submission.SubmittedAtUtc)
            .AsSplitQuery()
            .ToListAsync();

    public Task<List<Submission>> GetForClientAsync(Guid clientId, IReadOnlyCollection<int> exerciseIds) =>
        _dataContext.Submissions
            .AsNoTracking()
            .Where(submission => submission.ClientId == clientId && exerciseIds.Contains(submission.ExerciseId))
            .OrderBy(submission => submission.SubmittedAtUtc)
            .ToListAsync();
}
