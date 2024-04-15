using MMSA.BLL.Dtos;

namespace MMSA.BLL.Services.Interfaces
{
    public interface IPageService
    {
        Task<PageDTO> CreateAsync(PageDTO page);
    }
}
