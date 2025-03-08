using System.Net.Http.Headers;

namespace Olve.Proxmox.Authentication;

public static class HttpRequestHeadersExtensions
{
    public static void AddProxmoxAuthenticationHeader(this HttpRequestHeaders headers, ProxmoxConnectionInfo connectionInfo)
    {
        headers.Authorization = AuthenticationHelper.GetProxmoxAuthenticationHeader(connectionInfo);
    }
}