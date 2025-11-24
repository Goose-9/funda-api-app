namespace FundaStats.Core.RateLimiting;

public sealed class FixedDelayRateLimiter : IRateLimiter
{
    private readonly TimeSpan _delay;

    public FixedDelayRateLimiter(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay));

        _delay = delay;
    }

    public async ValueTask WaitAsync(CancellationToken token)
    {
        if (_delay == TimeSpan.Zero)
            return;

        await Task.Delay(_delay, token);
    }
}
