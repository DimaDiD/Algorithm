using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MMSA.BLL.Dtos;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation
{
    public class PageContentService: IPageContentService
    {
        private const string CodeType = "Code";
        private const int MaxTopicLength = 100;
        private const int MaxTopicsPerBlock = 15;

        private readonly IPageContentRepository _pageContentRepository;
        private readonly IProgrammingTopicRepository _programmingTopicRepository;
        private readonly IMapper _mapper;

        public PageContentService(
            IPageContentRepository pageContentRepository,
            IProgrammingTopicRepository programmingTopicRepository,
            IMapper mapper)
        {
            _pageContentRepository = pageContentRepository;
            _programmingTopicRepository = programmingTopicRepository;
            _mapper = mapper;
        }

        public async Task<List<PageContentDTO>> GetPageContentBySettingStatusAsync(int pageId, int? subPageId, string codeStatus)
        {
            try
            {
                var pageContent = subPageId == null
                    ? await _pageContentRepository.GetAllAsync(
                        x => x.PageId == pageId && (x.CodeType == codeStatus || x.CodeType == "All"),
                        include: query => query.Include(x => x.Topics))
                    : await _pageContentRepository.GetAllAsync(
                        x => x.SubPageId == subPageId && (x.CodeType == codeStatus || x.CodeType == "All"),
                        include: query => query.Include(x => x.Topics));

                return _mapper.Map<List<PageContentDTO>>(pageContent.OrderBy(x => x.ContentLocation));
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task CreatePageContentAsync(PageContentDTO pageContentDto)
        {
            try
            {
                var topicNames = NormalizeTopics(pageContentDto);
                var pageContent = _mapper.Map<PageContent>(pageContentDto);

                var pageContents = await _pageContentRepository.GetAllAsync(x => pageContent.SubPageId == x.SubPageId && 
                    pageContent.PageId  == x.PageId && x.ContentLocation >= pageContent.ContentLocation);

                foreach (var item in pageContents) {
                    item.ContentLocation += 1;
                    await _pageContentRepository.UpdateAsync(item, true); 
                }

                foreach (var topic in await _programmingTopicRepository.GetOrAddByNamesAsync(topicNames))
                    pageContent.Topics.Add(topic);

                await _pageContentRepository.InsertAsync(pageContent, true);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task UpdatePageContentAsync(PageContentDTO pageContentDto)
        {
            try
            {
                var topicNames = NormalizeTopics(pageContentDto);
                var pageContent = await _pageContentRepository.GetWithTopicsAsync(pageContentDto.Id)
                    ?? throw new KeyNotFoundException("Блок не знайдено.");

                pageContent.Text = pageContentDto.Text;
                pageContent.TextType = pageContentDto.TextType;
                pageContent.CodeType = pageContentDto.CodeType;

                var topics = await _programmingTopicRepository.GetOrAddByNamesAsync(topicNames);
                pageContent.Topics.Clear();
                foreach (var topic in topics)
                    pageContent.Topics.Add(topic);

                await _pageContentRepository.SaveAsync();
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task DeletePageContentAsync(int pageContentId)
        {
            try
            {
                var pageContent = await _pageContentRepository.GetFirstOrDefaultAsync(x => x.Id == pageContentId);
                await _pageContentRepository.DeleteAsync(pageContent, true);

                var pageContents = await _pageContentRepository.GetAllAsync(x => pageContent.SubPageId == x.SubPageId && pageContent.PageId == x.PageId && x.ContentLocation >= pageContent.ContentLocation);
                foreach (var item in pageContents)
                {
                    item.ContentLocation -= 1;
                    await _pageContentRepository.UpdateAsync(item, true);
                }                               
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public Task<List<string>> GetProgrammingTopicsAsync() =>
            _programmingTopicRepository.GetAllNamesAsync();

        private static string[] NormalizeTopics(PageContentDTO pageContent)
        {
            if (pageContent.TextType != CodeType)
                return Array.Empty<string>();

            var names = pageContent.Topics
                .Select(name => name?.Trim() ?? string.Empty)
                .Where(name => name.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (names.Length > MaxTopicsPerBlock)
                throw new ArgumentException($"Можна вказати не більше {MaxTopicsPerBlock} тем.");

            if (names.Any(name => name.Length > MaxTopicLength))
                throw new ArgumentException($"Назва теми не може бути довшою за {MaxTopicLength} символів.");

            return names;
        }
    }
}
