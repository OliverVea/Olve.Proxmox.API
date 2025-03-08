using Olve.Proxmox.Authentication;
using Olve.Utilities.Operations;

namespace Olve.Proxmox.Operations;

public class CheckProxmoxConnectionOperation(ProxmoxHttpService proxmoxHttpService) : IAsyncOperation<CheckProxmoxConnectionOperation.Request>
{
    public record Request(ProxmoxConnectionInfo ConnectionInfo);

    public Task<Result> ExecuteAsync(Request request, CancellationToken ct = new())
    {
        return proxmoxHttpService.ExecuteAsync(HttpRequest, request, ct: ct);
    }

    private static async ValueTask<Result> HttpRequest(HttpClient httpClient, Request request, CancellationToken ct)
    {
        var url = ProxmoxUrlHelper.ListNodesUrl(request.ConnectionInfo);

        httpClient.DefaultRequestHeaders.AddProxmoxAuthenticationHeader(request.ConnectionInfo);

        var response = await httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ResultProblem("Failed to connect to Proxmox API");
        }

        return Result.Success();
    }
}