using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface IProgrammingTopicRepository : IBaseRepository<ProgrammingTopic>
{
    Task<List<string>> GetAllNamesAsync();
    Task<List<ProgrammingTopic>> GetOrAddByNamesAsync(IReadOnlyCollection<string> names);
}
