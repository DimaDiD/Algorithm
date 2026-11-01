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

            var siblings = await _subPageRepository.GetAllAsync(s => s.PageId == pageId);
            var order = siblings.Any() ? siblings.Max(s => s.Order) + 1 : 0;

            await _subPageRepository.InsertAsync(new SubPage { PageId = pageId, Name = name, Order = order }, true);
        }

        public async Task ReorderSubPagesAsync(int pageId, IReadOnlyList<int> orderedSubPageIds)
        {
            var subPages = (await _subPageRepository.GetAllAsync(s => s.PageId == pageId)).ToList();
            PageService.EnsureSameSet(subPages.Select(s => s.Id), orderedSubPageIds, "тем");

            var positions = orderedSubPageIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);
            foreach (var subPage in subPages.Where(s => s.Order != positions[s.Id]))
            {
                subPage.Order = positions[subPage.Id];
                await _subPageRepository.UpdateAsync(subPage, false);
            }

            await _subPageRepository.SaveAsync();
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
