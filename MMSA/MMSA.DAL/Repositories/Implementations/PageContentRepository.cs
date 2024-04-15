using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations
{
    public class PageContentRepository : BaseRepository<PageContent>, IPageContentRepository
    {
        public PageContentRepository(AlgorithmDataContext dataContext) : base(dataContext)
        {

        }
    }
}
