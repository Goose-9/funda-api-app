namespace FundaStats.Core.RateLimiting;

public interface IRateLimiter
{
    ValueTask WaitAsync(CancellationToken token = default);
}
