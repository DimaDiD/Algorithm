using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Interfaces
{
    public interface IPageContentService
    {
        Task<List<PageContent>> GetPageContentBySettingStatusAsync(int pageId, int? subPageId);
        Task CreatePageContentAsync(PageContent pageContent);
        Task DeletePageContentAsync(PageContent pageContent);
    }
}
