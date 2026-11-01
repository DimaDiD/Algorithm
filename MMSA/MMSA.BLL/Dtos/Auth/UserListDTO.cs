namespace MMSA.BLL.Dtos.Auth;

public class UserListItemDTO
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CourseCount { get; set; }
    public bool IsCurrentUser { get; set; }
}

public class UserPageDTO
{
    public List<UserListItemDTO> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
