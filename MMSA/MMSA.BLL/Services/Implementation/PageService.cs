using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MMSA.BLL.Dtos;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation
{
    public class PageService : IPageService
    {
        private const int MaxNameLength = 100;

        private readonly IPageRepository _pageRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public PageService(IPageRepository pageRepository, ICourseRepository courseRepository, IMapper mapper)
        {
            _pageRepository = pageRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<List<PageDTO>> GetMenuItemsAsync(int courseId)
        {
            var pages = await _pageRepository.GetAllAsync(
                page => page.CourseId == courseId,
                include: query => query.Include(page => page.SubPages));

            var result = _mapper.Map<List<PageDTO>>(pages.OrderBy(page => page.Id));
            foreach (var page in result)
                page.SubPages = page.SubPages.OrderBy(subPage => subPage.Id).ToList();

            return result;
        }

        public async Task CreatePageAsync(int courseId, string pageName)
        {
            var name = ValidateName(pageName);

            if (await _pageRepository.GetFirstOrDefaultAsync(page => page.CourseId == courseId && page.PageName == name) is not null)
                throw new ArgumentException($"Розділ «{name}» уже є в цьому курсі.");

            await _pageRepository.InsertAsync(new Page { PageName = name, CourseId = courseId }, true);
        }

        public async Task RenamePageAsync(int pageId, string newName)
        {
            var name = ValidateName(newName);

            var page = await _pageRepository.GetFirstOrDefaultAsync(p => p.Id == pageId)
                ?? throw new KeyNotFoundException("Розділ не знайдено.");

            if (await _pageRepository.GetFirstOrDefaultAsync(p => p.CourseId == page.CourseId && p.Id != pageId && p.PageName == name) is not null)
                throw new ArgumentException($"Розділ «{name}» уже є в цьому курсі.");

            page.PageName = name;
            await _pageRepository.UpdateAsync(page, true);
        }

        public Task DeletePageAsync(int pageId) => _courseRepository.DeletePageTreeAsync(pageId);

        internal static string ValidateName(string? value)
        {
            var name = (value ?? string.Empty).Trim();
            if (name.Length == 0 || name.Length > MaxNameLength)
                throw new ArgumentException($"Назва: від 1 до {MaxNameLength} символів.");
            if (name.Contains('/'))
                throw new ArgumentException("Назва не може містити символ «/».");
            return name;
        }
    }
}
