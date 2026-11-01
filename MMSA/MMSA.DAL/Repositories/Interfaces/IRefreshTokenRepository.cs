using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<int> DeleteExpiredBeforeAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}