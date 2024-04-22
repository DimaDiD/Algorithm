using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Interfaces
{
    public interface IPageContentService
    {
        Task<List<PageContent>> GetPageContentBySettingStatusAsync(int pageId, int? subPageId, string codeStatus);
        Task CreatePageContentAsync(PageContent pageContent);
        Task DeletePageContentAsync(int pageContentId);
        Task UpdatePageContentAsync(PageContent pageContent);
    }
}
