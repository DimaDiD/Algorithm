using AutoMapper;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation
{
    public class PageContentService: IPageContentService
    {
        private readonly IPageContentRepository _pageContentRepository;
        private readonly IMapper _mapper;
        public PageContentService(IPageContentRepository pageContentRepository,
            IMapper mapper)
        {
            _pageContentRepository = pageContentRepository;
            _mapper = mapper;
        }

        public async Task<List<PageContent>> GetPageContentBySettingStatusAsync(int pageId, int? subPageId)
        {
            try
            {
                if (subPageId == null)
                {
                    var pageContent = await _pageContentRepository.GetAllAsync(x => x.PageId == pageId);
                    return pageContent.OrderBy(x => x.ContentLocation).ToList();
                }
                else
                {
                    var pageContent = await _pageContentRepository.GetAllAsync(x => x.SubPageId == subPageId);
                    return pageContent.OrderBy(x => x.ContentLocation).ToList();
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create CreateSubPageAsync page method. Message: {exception.Message}");
            }
        }

        public async Task CreatePageContentAsync(PageContent pageContent)
        {
            try
            {
                await _pageContentRepository.InsertAsync(pageContent, true);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create CreateSubPageAsync page method. Message: {exception.Message}");
            }
        }

        public async Task DeletePageContentAsync(PageContent pageContent)
        {
            try
            {
                await _pageContentRepository.DeleteAsync(pageContent, true);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create CreateSubPageAsync page method. Message: {exception.Message}");
            }
        }
    }
}
