using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Olve.Proxmox.API;
using Polly;
using Polly.Retry;

var serviceCollection = new ServiceCollection();

serviceCollection.AddHttpClient();
serviceCollection.AddResiliencePipeline(CheckProxmoxConnectionOperation.ResiliencePipelineKey, builder =>
{
    builder.AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>().HandleInner<TimeoutException>(),
            Delay = TimeSpan.FromSeconds(0.5),
            MaxRetryAttempts = 4, // Try 5 times total
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        })
        .AddTimeout(TimeSpan.FromSeconds(30));
});

serviceCollection.AddSingleton<CheckProxmoxConnectionOperation>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var checkProxmoxConnectionOperation = serviceProvider.GetRequiredService<CheckProxmoxConnectionOperation>();

var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var apiKeyFile = Path.Combine(assemblyDir, ".proxmox-api-key");

var apiKey = File.ReadAllText(apiKeyFile).Trim();

ProxmoxConnectionInfo connectionInfo = new("fortress.lan", 8006, apiKey);
var result = await checkProxmoxConnectionOperation.ExecuteAsync(new CheckProxmoxConnectionOperation.Request(connectionInfo));

if (result.TryPickProblems(out var problems))
{
    foreach (var problem in problems)
    {
        Console.WriteLine(problem);
    }

    return 1;
}

return 0;
