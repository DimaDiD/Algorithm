using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation
{
    public class SubPageService: ISubPageService
    {
        private readonly ISubPageRepository _subPageRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IMapper _mapper;
        public SubPageService(ISubPageRepository subPageRepository,
            IMapper mapper,
            IPageRepository pageRepository)
        {
            _subPageRepository = subPageRepository;
            _mapper = mapper;
            _pageRepository = pageRepository;
        }

        public async Task CreateSubPageAsync(string pageName, string subPageName)
        {
            try
            {
                var page = await _pageRepository.GetFirstOrDefaultAsync(x => x.PageName == pageName);
                var newSubPage = new SubPage
                {
                    PageId = page.Id,
                    Name = subPageName
                };

                await _subPageRepository.InsertAsync(newSubPage, true);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create CreateSubPageAsync page method. Message: {exception.Message}");
            }
        }

        public async Task UpdateMenuItemAsync(string oldTitle, string newTitle)
        {
            try
            {
                var page = await _pageRepository.GetFirstOrDefaultAsync(x => x.PageName == oldTitle);

                if (page != null)
                {
                    page.PageName = newTitle;
                    await _pageRepository.UpdateAsync(page, true);
                }
                else
                {
                    var subPage = await _subPageRepository.GetFirstOrDefaultAsync(x => x.Name == oldTitle);

                    subPage.Name = newTitle;
                    await _subPageRepository.UpdateAsync(subPage, true);
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create CreateSubPageAsync page method. Message: {exception.Message}");
            }
        }

        public async Task DeleteMenuItemAsync( string title)
        {
            try
            {
                var page = await _pageRepository.GetFirstOrDefaultAsync(x => x.PageName == title, include: x => x.Include(x => x.SubPages));

                if (page != null)
                {   
                    //DeleteContent

                    foreach (var subPage in page.SubPages)
                    {
                        await _subPageRepository.DeleteAsync(subPage, true);
                    }

                    await _pageRepository.DeleteAsync(page, true);

                    
                }
                else
                {
                    //deleteContent 

                    var subPage = await _subPageRepository.GetFirstOrDefaultAsync(x => x.Name == title);

                    await _subPageRepository.DeleteAsync(subPage, true);
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create CreateSubPageAsync page method. Message: {exception.Message}");
            }
        }
    }
}
