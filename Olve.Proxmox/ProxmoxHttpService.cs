using System.Security.Authentication;
using Polly.Registry;

namespace Olve.Proxmox;

public class ProxmoxHttpService(IHttpClientFactory httpClientFactory, ResiliencePipelineProvider<string> resiliencePipelineProvider)
{
    public const string DefaultResiliencePipelineKey = "ProxmoxHttpService";

    public async Task<Result> ExecuteAsync<TState>(
        Func<HttpClient, TState, CancellationToken, ValueTask<Result>> action,
        TState state,
        string resiliencePipelineKey = DefaultResiliencePipelineKey,
        CancellationToken ct = default)
    {
        var resiliencePolicy = resiliencePipelineProvider.GetPipeline(resiliencePipelineKey);
        var httpClient = httpClientFactory.CreateClient();
        try
        {
            return await resiliencePolicy.ExecuteAsync<Result, (HttpClient, TState)>(
                async (tuple, token) => await action(tuple.Item1, tuple.Item2, token),
                (httpClient, state),
                ct);
        }
        catch (HttpRequestException httpRequestException)
        {
            if (httpRequestException.InnerException is AuthenticationException authenticationException)
            {
                 return new ResultProblem(authenticationException, "Failed to authenticate with Proxmox API");
            }

            throw;
        }
    }

    public async Task<Result<TResult>> ExecuteAsync<TState, TResult>(
        Func<HttpClient, TState, CancellationToken, ValueTask<Result<TResult>>> action,
        TState state,
        string resiliencePipelineKey = DefaultResiliencePipelineKey,
        CancellationToken ct = default)
    {
        var resiliencePolicy = resiliencePipelineProvider.GetPipeline(resiliencePipelineKey);
        var httpClient = httpClientFactory.CreateClient();
        try
        {
            return await resiliencePolicy.ExecuteAsync<Result<TResult>, (HttpClient, TState)>(
                async (tuple, token) => await action(tuple.Item1, tuple.Item2, token),
                (httpClient, state),
                ct);
        }
        catch (HttpRequestException httpRequestException)
        {
            if (httpRequestException.InnerException is AuthenticationException authenticationException)
            {
                return new ResultProblem(authenticationException, "Failed to authenticate with Proxmox API");
            }

            throw;
        }
    }
}