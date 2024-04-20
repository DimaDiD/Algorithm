namespace MMSA.BLL.Services.Interfaces
{
    public interface ISubPageService
    {
        Task CreateSubPageAsync(string pageName, string subPage);
        Task UpdateMenuItemAsync(string oldTitle, string newTitle);
        Task DeleteMenuItemAsync(string title);
    }
}
