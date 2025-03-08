using System.Net.Http.Json;
using Olve.Proxmox.Authentication;
using Olve.Utilities.Operations;

namespace Olve.Proxmox.Operations;

public readonly record struct MappingId(string Value);
public readonly record struct PCIMapping(MappingId Id, string? Description);

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
            responseModel.Data.Select(x => new PCIMapping(new MappingId(x.Id), x.Description)).ToList());
    }

    /*
     {
         "data" : [ {
           "type" : "pci",
           "map" : [ "id=8086:7af0,iommugroup=6,node=fortress,path=0000:00:14.3,subsystem-id=8086:0074" ],
           "digest" : "e45779c584628a4488f935587e5ea459af58d1c5",
           "id" : "WifiCard"
         }, {
           "id" : "RTX3070",
           "digest" : "e45779c584628a4488f935587e5ea459af58d1c5",
           "map" : [ "id=10de:2488,iommugroup=14,node=fortress,path=0000:01:00,subsystem-id=1043:883a" ],
           "description" : "NVidia RTX3070 GPU",
           "type" : "pci"
         } ]
       }
     */

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
