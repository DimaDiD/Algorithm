namespace MMSA.BLL.Services.Interfaces
{
    public interface ISubPageService
    {
        Task CreateSubPageAsync(int pageId, string subPageName);
        Task RenameSubPageAsync(int subPageId, string newName);
        Task DeleteSubPageAsync(int subPageId);
    }
}
