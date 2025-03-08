using System.Net.Http.Json;
using Olve.Proxmox.Authentication;
using Olve.Proxmox.Models;
using Olve.Utilities.Operations;

namespace Olve.Proxmox.Operations;

public class ListPCIMappingsOperation(ProxmoxHttpService proxmoxHttpService)
    : IAsyncOperation<ListPCIMappingsOperation.Request, ListPCIMappingsOperation.Response>
{
    public record Request(ProxmoxConnectionInfo ConnectionInfo);

    public record Response(IReadOnlyList<PCIMapping> PCIMappings);

    public Task<Result<Response>> ExecuteAsync(Request request, CancellationToken ct = default)
    {
        return proxmoxHttpService.ExecuteAsync(HttpRequest, request, ct: ct);
    }

    private static async ValueTask<Result<Response>> HttpRequest(HttpClient httpClient, Request request,
        CancellationToken ct)
    {
        var url = ProxmoxUrlHelper.ListPCIMappings(request.ConnectionInfo);

        var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.AddProxmoxAuthenticationHeader(request.ConnectionInfo);

        var response = await httpClient.SendAsync(message, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ResultProblem("Failed to connect to Proxmox API");
        }

        var responseModel = await response.Content.ReadFromJsonAsync<ResponseModel>(ct);

        if (responseModel is null)
        {
            return new ResultProblem("Failed to deserialize response from Proxmox API");
        }

        return MapResponse(responseModel);
    }

    private static Response MapResponse(ResponseModel responseModel)
    {
        return new Response(
            responseModel.Data.Select(x => new PCIMapping(new PCIMappingId(x.Id), x.Description)).ToList());
    }

    private class ResponseModel
    {
        public MappingModel[] Data { get; set; } = [];

    }

    public class MappingModel
    {
        public string Type { get; set; } = string.Empty;
        public string[] Map { get; set; } = [];
        public string Digest { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
    }
}
