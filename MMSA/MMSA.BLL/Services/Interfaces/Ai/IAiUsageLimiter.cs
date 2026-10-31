namespace MMSA.BLL.Services.Interfaces.Ai;

public interface IAiUsageLimiter
{
    bool TryAcquire(string clientKey);
}
