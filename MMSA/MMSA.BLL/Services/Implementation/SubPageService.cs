using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation
{
    public class SubPageService : ISubPageService
    {
        private readonly ISubPageRepository _subPageRepository;
        private readonly ICourseRepository _courseRepository;

        public SubPageService(ISubPageRepository subPageRepository, ICourseRepository courseRepository)
        {
            _subPageRepository = subPageRepository;
            _courseRepository = courseRepository;
        }

        public async Task CreateSubPageAsync(int pageId, string subPageName)
        {
            var name = PageService.ValidateName(subPageName);

            if (await _subPageRepository.GetFirstOrDefaultAsync(s => s.PageId == pageId && s.Name == name) is not null)
                throw new ArgumentException($"Тема «{name}» уже є в цьому розділі.");

            await _subPageRepository.InsertAsync(new SubPage { PageId = pageId, Name = name }, true);
        }

        public async Task RenameSubPageAsync(int subPageId, string newName)
        {
            var name = PageService.ValidateName(newName);

            var subPage = await _subPageRepository.GetFirstOrDefaultAsync(s => s.Id == subPageId)
                ?? throw new KeyNotFoundException("Тему не знайдено.");

            if (await _subPageRepository.GetFirstOrDefaultAsync(s => s.PageId == subPage.PageId && s.Id != subPageId && s.Name == name) is not null)
                throw new ArgumentException($"Тема «{name}» уже є в цьому розділі.");

            subPage.Name = name;
            await _subPageRepository.UpdateAsync(subPage, true);
        }

        public Task DeleteSubPageAsync(int subPageId) => _courseRepository.DeleteSubPageTreeAsync(subPageId);
    }
}
