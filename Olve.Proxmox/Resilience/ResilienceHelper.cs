using Polly;
using Polly.Retry;

namespace Olve.Proxmox.Resilience;

public static class ResilienceHelper
{
    public static ResiliencePipelineBuilder AddDefaultResiliencePolicies(this ResiliencePipelineBuilder builder)
    {
        return builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>().HandleInner<TimeoutException>(),
                Delay = TimeSpan.FromSeconds(0.5),
                MaxRetryAttempts = 4,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddTimeout(TimeSpan.FromSeconds(30));
    }
}