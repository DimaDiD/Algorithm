using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class ExerciseRepository : BaseRepository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(AlgorithmDataContext dataContext) : base(dataContext)
    {
    }

    public Task<List<Exercise>> GetByTopicAsync(int pageId, int? subPageId) =>
        _dataContext.Exercises
            .AsNoTracking()
            .Where(e => e.PageId == pageId && e.SubPageId == subPageId)
            .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
            .OrderBy(e => e.Order)
            .AsSplitQuery()
            .ToListAsync();

    public Task<Exercise?> GetWithQuestionsAsync(int id, bool track = false)
    {
        var query = _dataContext.Exercises
            .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
            .AsSplitQuery();

        return (track ? query : query.AsNoTracking()).FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<int> GetNextOrderAsync(int pageId, int? subPageId)
    {
        var max = await _dataContext.Exercises
            .Where(e => e.PageId == pageId && e.SubPageId == subPageId)
            .MaxAsync(e => (int?)e.Order);

        return (max ?? -1) + 1;
    }
}
