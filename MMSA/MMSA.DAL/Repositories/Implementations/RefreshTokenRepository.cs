using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AlgorithmDataContext dataContext) : base(dataContext)
    {
    }

    public Task<int> DeleteExpiredBeforeAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default) =>
        _dataContext.RefreshTokens
            .Where(token => token.ExpiresAtUtc < cutoffUtc)
            .ExecuteDeleteAsync(cancellationToken);
}