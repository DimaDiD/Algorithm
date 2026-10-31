using MMSA.BLL.Dtos.Reviews;

namespace MMSA.BLL.Services.Interfaces;

public interface ICourseDashboardService
{
    Task<CourseDashboardDTO> GetDashboardAsync(int courseId);
    Task<ReviewPageDTO> GetReviewsAsync(int courseId, int? exerciseId, string? status, int page);
    Task ReviewAsync(int answerId, ReviewUpdateDTO review, string? reviewerId);
}
