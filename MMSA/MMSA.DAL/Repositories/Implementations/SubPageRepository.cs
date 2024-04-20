using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations
{
    public class SubPageRepository : BaseRepository<SubPage>, ISubPageRepository
    {
        public SubPageRepository(AlgorithmDataContext dataContext) : base(dataContext)
        {

        }
    }
}
