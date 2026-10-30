using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class ProgrammingTopicRepository : BaseRepository<ProgrammingTopic>, IProgrammingTopicRepository
{
    public ProgrammingTopicRepository(AlgorithmDataContext dataContext) : base(dataContext)
    {
    }

    public Task<List<string>> GetAllNamesAsync() =>
        _dataContext.ProgrammingTopics
            .AsNoTracking()
            .OrderBy(topic => topic.Name)
            .Select(topic => topic.Name)
            .ToListAsync();

    public async Task<List<ProgrammingTopic>> GetOrAddByNamesAsync(IReadOnlyCollection<string> names)
    {
        if (names.Count == 0)
            return new List<ProgrammingTopic>();

        var requested = names.ToArray();
        var known = await _dataContext.ProgrammingTopics
            .Where(topic => requested.Contains(topic.Name))
            .ToListAsync();

        var result = new List<ProgrammingTopic>();
        foreach (var name in requested)
        {
            var topic = known.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
            if (topic is null)
            {
                topic = new ProgrammingTopic { Name = name };
                _dataContext.ProgrammingTopics.Add(topic);
                known.Add(topic);
            }
            result.Add(topic);
        }

        return result;
    }
}
