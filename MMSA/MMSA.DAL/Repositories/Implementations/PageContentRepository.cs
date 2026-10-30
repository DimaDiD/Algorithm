using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations
{
    public class PageContentRepository : BaseRepository<PageContent>, IPageContentRepository
    {
        public PageContentRepository(AlgorithmDataContext dataContext) : base(dataContext)
        {

        }

        public Task<PageContent?> GetWithTopicsAsync(int id) =>
            _dataContext.PageContents
                .Include(content => content.Topics)
                .FirstOrDefaultAsync(content => content.Id == id);
    }
}
