using Microsoft.Extensions.Options;
using MMSA.BLL.Options;
using MMSA.BLL.Services.Interfaces.Ai;

namespace MMSA.BLL.Services.Implementation.Ai;

public sealed class AiUsageLimiter : IAiUsageLimiter
{
    private readonly object _sync = new();
    private readonly Dictionary<string, int> _perClient = new();
    private readonly AiReviewOptions _options;
    private readonly TimeProvider _timeProvider;
    private DateOnly _day;
    private int _total;

    public AiUsageLimiter(IOptions<AiReviewOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public bool TryAcquire(string clientKey)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        lock (_sync)
        {
            if (today != _day)
            {
                _day = today;
                _total = 0;
                _perClient.Clear();
            }

            var used = _perClient.GetValueOrDefault(clientKey);
            if (_total >= _options.DailyLimitTotal || used >= _options.DailyLimitPerClient)
                return false;

            _perClient[clientKey] = used + 1;
            _total++;
            return true;
        }
    }
}
