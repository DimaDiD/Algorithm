using MMSA.BLL.Dtos;

namespace MMSA.BLL.Services.Interfaces
{
    public interface IPageService
    {
        Task<List<PageDTO>> GetMenuItemsAsync(int courseId);
        Task CreatePageAsync(int courseId, string pageName);
        Task RenamePageAsync(int pageId, string newName);
        Task DeletePageAsync(int pageId);
    }
}
