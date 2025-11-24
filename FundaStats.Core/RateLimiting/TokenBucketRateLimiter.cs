namespace FundaStats.Core.RateLimiting;

public sealed class TokenBucketRateLimiter : IRateLimiter
{
    private readonly object _lock = new();

    private readonly double _maxTokens;
    private readonly double _tokensPerMs;

    private double _availableTokens;
    private DateTime _lastRefill;

    // getters to use in tests
    internal double AvailableTokens => _availableTokens;
    internal DateTime LastRefill => _lastRefill;
    internal double MaxTokens => _maxTokens;
    internal double TokensPerMillisecond => _tokensPerMs;

    public TokenBucketRateLimiter(int maxTokensPerMinute)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTokensPerMinute);
        _maxTokens = maxTokensPerMinute;
        _tokensPerMs = maxTokensPerMinute / 60000d;
        _availableTokens = 0;
        _lastRefill = DateTime.UtcNow;
    }

    public async ValueTask WaitAsync(CancellationToken token = default)
    {
        while (true)
        {
            double delayMs;

            lock (_lock)
            {
                RefillTokens();

                if (_availableTokens >= 1.0)
                {
                    _availableTokens -= 1.0;
                    return;
                }

                // Checks how long until the bucket has at least 1 token
                var tokensNeeded = 1.0 - _availableTokens;
                delayMs = tokensNeeded / _tokensPerMs;

                if (delayMs < 0)
                    delayMs = 0;
            }

            // Wait outside lock to avoid blocking others
            if (delayMs > 0)
            {
                TimeSpan delay = TimeSpan.FromMilliseconds(delayMs);
                await Task.Delay(delay, token);
            }
            else
            {
                await Task.Yield();
            }
        }
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var elapsedMs = (now - _lastRefill).TotalMilliseconds;

        if (elapsedMs <= 0)
            return;

        _availableTokens = Math.Min(_maxTokens, _availableTokens + elapsedMs * _tokensPerMs);

        _lastRefill = now;
    }
}
