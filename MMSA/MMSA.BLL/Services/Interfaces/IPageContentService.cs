using MMSA.BLL.Dtos;

namespace MMSA.BLL.Services.Interfaces
{
    public interface IPageContentService
    {
        Task<List<PageContentDTO>> GetPageContentBySettingStatusAsync(int pageId, int? subPageId, string codeStatus);
        Task CreatePageContentAsync(PageContentDTO pageContent);
        Task DeletePageContentAsync(int pageContentId);
        Task UpdatePageContentAsync(PageContentDTO pageContent);
        Task<List<string>> GetProgrammingTopicsAsync();
    }
}
