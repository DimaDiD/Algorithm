using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MMSA.BLL.Dtos;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation
{
    public class PageService: IPageService
    {
        private readonly IPageRepository _pageRepository;
        private readonly IMapper _mapper;
        public PageService(IPageRepository pageRepository,
            IMapper mapper) 
        {
            _pageRepository = pageRepository;
            _mapper = mapper;
        }

        public async Task<List<PageDTO>> GetMenuItemsAsync()
        {
            try
            {
                var dbPages = await _pageRepository.GetAllAsync(include: x => x.Include(c => c.SubPages));
                return _mapper.Map<List<PageDTO>>(dbPages);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create GetAllAsync page method. Message: {exception.Message}");
            }
        }

        public async Task CreatePageAsync(string pageName)
        {
            try
            {
                var newPage = new Page
                {
                    PageName = pageName,
                };

                await _pageRepository.InsertAsync(newPage, true);                
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create GetAllAsync page method. Message: {exception.Message}");
            }
        }

        public async Task<List<PageDTO>> GetContentAsync()
        {
            try
            {
                var dbPages = await _pageRepository.GetAllAsync(include: x => x.Include(c => c.SubPages));
                return _mapper.Map<List<PageDTO>>(dbPages);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create GetAllAsync page method. Message: {exception.Message}");
            }
        }
    }
}
