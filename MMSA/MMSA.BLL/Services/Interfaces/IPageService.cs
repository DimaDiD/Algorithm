using MMSA.BLL.Dtos;
using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Interfaces
{
    public interface IPageService
    {
        Task<PageDTO> CreateAsync(PageDTO page);
        Task<List<PageDTO>> GetMenuItemsAsync();
    }
}
