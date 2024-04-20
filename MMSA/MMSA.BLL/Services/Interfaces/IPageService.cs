using MMSA.BLL.Dtos;
using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Interfaces
{
    public interface IPageService
    {
        Task CreatePageAsync(string pageName);
        Task<List<PageDTO>> GetMenuItemsAsync();
    }
}
