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
        private const string AllLanguages = "All";
        private const int MaxTopicLength = 100;
        private const int MaxTopicsPerBlock = 15;

        private readonly IPageContentRepository _pageContentRepository;
        private readonly IProgrammingTopicRepository _programmingTopicRepository;
        private readonly IMapper _mapper;
        private readonly IHtmlContentSanitizer _htmlSanitizer;

        public PageContentService(
            IPageContentRepository pageContentRepository,
            IProgrammingTopicRepository programmingTopicRepository,
            IMapper mapper,
            IHtmlContentSanitizer htmlSanitizer)
        {
            _pageContentRepository = pageContentRepository;
            _programmingTopicRepository = programmingTopicRepository;
            _mapper = mapper;
            _htmlSanitizer = htmlSanitizer;
        }

        public async Task<List<PageContentDTO>> GetPageContentBySettingStatusAsync(int pageId, int? subPageId, string codeStatus)
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

        public async Task CreatePageContentAsync(PageContentDTO pageContentDto)
        {
            var topicNames = NormalizeTopics(pageContentDto);
            var pageContent = _mapper.Map<PageContent>(pageContentDto);
            pageContent.Text = SanitizeText(pageContentDto);

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

        public async Task UpdatePageContentAsync(PageContentDTO pageContentDto)
        {
            var topicNames = NormalizeTopics(pageContentDto);
            var pageContent = await _pageContentRepository.GetWithTopicsAsync(pageContentDto.Id)
                ?? throw new KeyNotFoundException("Блок не знайдено.");

            pageContent.Text = SanitizeText(pageContentDto);
            pageContent.TextType = pageContentDto.TextType;
            pageContent.CodeType = pageContentDto.CodeType;

            var topics = await _programmingTopicRepository.GetOrAddByNamesAsync(topicNames);
            pageContent.Topics.Clear();
            foreach (var topic in topics)
                pageContent.Topics.Add(topic);

            await _pageContentRepository.SaveAsync();
        }

        public async Task DeletePageContentAsync(int pageContentId)
        {
            var pageContent = await _pageContentRepository.GetFirstOrDefaultAsync(x => x.Id == pageContentId)
                ?? throw new KeyNotFoundException("Блок не знайдено.");
            await _pageContentRepository.DeleteAsync(pageContent, true);

            var pageContents = await _pageContentRepository.GetAllAsync(x => pageContent.SubPageId == x.SubPageId && pageContent.PageId == x.PageId && x.ContentLocation >= pageContent.ContentLocation);
            foreach (var item in pageContents)
            {
                item.ContentLocation -= 1;
                await _pageContentRepository.UpdateAsync(item, true);
            }                               
        }

        public async Task MovePageContentAsync(int pageContentId, MoveDirection direction, string codeStatus)
        {
            var target = await _pageContentRepository.GetFirstOrDefaultAsync(x => x.Id == pageContentId)
                ?? throw new KeyNotFoundException("Блок не знайдено.");

            var sequence = (await _pageContentRepository.GetAllAsync(x => x.PageId == target.PageId && x.SubPageId == target.SubPageId))
                .OrderBy(x => x.ContentLocation)
                .ThenBy(x => x.Id)
                .ToList();
            var block = sequence.Single(x => x.Id == pageContentId);

            var visible = sequence
                .Where(x => x.Id == block.Id || x.CodeType == codeStatus || x.CodeType == AllLanguages)
                .ToList();
            var index = visible.IndexOf(block);
            var neighbourIndex = direction == MoveDirection.Up ? index - 1 : index + 1;
            if (neighbourIndex < 0 || neighbourIndex >= visible.Count)
                return;

            sequence.Remove(block);
            var neighbourPosition = sequence.IndexOf(visible[neighbourIndex]);
            var (start, end) = block.TextType == CodeType
                ? (neighbourPosition, neighbourPosition)
                : CodeRunAround(sequence, neighbourPosition);
            sequence.Insert(direction == MoveDirection.Up ? start : end + 1, block);

            for (var position = 0; position < sequence.Count; position++)
            {
                if (sequence[position].ContentLocation == position)
                    continue;

                sequence[position].ContentLocation = position;
                await _pageContentRepository.UpdateAsync(sequence[position], false);
            }

            await _pageContentRepository.SaveAsync();
        }

        private static (int Start, int End) CodeRunAround(List<PageContent> sequence, int position)
        {
            if (sequence[position].TextType != CodeType)
                return (position, position);

            var start = position;
            while (start > 0 && sequence[start - 1].TextType == CodeType)
                start--;

            var end = position;
            while (end < sequence.Count - 1 && sequence[end + 1].TextType == CodeType)
                end++;

            return (start, end);
        }

        public Task<List<string>> GetProgrammingTopicsAsync() =>
            _programmingTopicRepository.GetAllNamesAsync();

        private string SanitizeText(PageContentDTO pageContent) =>
            pageContent.TextType == CodeType ? pageContent.Text : _htmlSanitizer.Sanitize(pageContent.Text);

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
