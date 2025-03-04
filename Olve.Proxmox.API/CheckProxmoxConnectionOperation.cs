using System.Security.Authentication;
using Olve.Utilities.Operations;
using Polly.Registry;

namespace Olve.Proxmox.API;

public readonly record struct ProxmoxConnectionInfo(string Host, int Port, string ApiKey);

public class CheckProxmoxConnectionOperation(IHttpClientFactory httpClientFactory, ResiliencePipelineProvider<string> resiliencePipelineProvider) : IAsyncOperation<CheckProxmoxConnectionOperation.Request>
{
    public const string ResiliencePipelineKey = "CheckProxmoxConnection";
    public record Request(ProxmoxConnectionInfo ConnectionInfo);

    public async Task<Result> ExecuteAsync(Request request, CancellationToken ct = new())
    {
        var resiliencePolicy = resiliencePipelineProvider.GetPipeline(ResiliencePipelineKey);
        var httpClient = httpClientFactory.CreateClient();
        try
        {
            return await resiliencePolicy.ExecuteAsync<Result, (HttpClient, Request)>(
                async (state, token) => await HttpRequest(state, token),
                (httpClient, request),
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

    private static async ValueTask<Result> HttpRequest((HttpClient HttpClient, Request Request) state, CancellationToken ct)
    {
        var (httpClient, request) = state;

        var url = GetUrl(request.ConnectionInfo);
        var response = await httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ResultProblem("Failed to connect to Proxmox API");
        }

        return Result.Success();
    }

    private static string GetUrl(ProxmoxConnectionInfo connectionInfo) => $"https://{connectionInfo.Host}:{connectionInfo.Port}/api2/json/nodes";
}