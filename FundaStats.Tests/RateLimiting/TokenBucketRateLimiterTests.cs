using System.Diagnostics;
using FundaStats.Core.RateLimiting;

namespace FundaStats.Tests.RateLimiting;

public class TokenBucketRateLimiterTests
{
    [Fact]
    public void Constructor_EmptyBucket()
    {
        const int maxPerMinute = 100;

        var limiter = new TokenBucketRateLimiter(maxPerMinute);

        Assert.Equal(maxPerMinute, limiter.MaxTokens);
        Assert.Equal(0, limiter.AvailableTokens, 3);
    }

    [Fact]
    public async Task WaitAsync_DelayWhenBucketEmpty()
    {
        const int maxPerMinute = 60; // 1 per second
        var limiter = new TokenBucketRateLimiter(maxPerMinute);

        var sw = Stopwatch.StartNew();
        await limiter.WaitAsync();
        sw.Stop();

        Assert.True(
            sw.ElapsedMilliseconds >= 500,
            $"Expected initial WaitAsync to wait, but took only {sw.ElapsedMilliseconds} ms."
        );
    }

    [Fact]
    public async Task WaitAsync_NoNegativeTokens()
    {
        const int maxPerMinute = 5;
        var limiter = new TokenBucketRateLimiter(maxPerMinute);

        for (int i = 0; i < 20; i++)
        {
            await limiter.WaitAsync();
            Assert.True(
                limiter.AvailableTokens >= -0.0001,
                $"AvailableTokens should not be significantly negative, but was {limiter.AvailableTokens}"
            );
        }
    }
}
