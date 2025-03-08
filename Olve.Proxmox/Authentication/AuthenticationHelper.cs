using System.Net.Http.Headers;

namespace Olve.Proxmox.Authentication;

public static class AuthenticationHelper
{
    public static AuthenticationHeaderValue GetProxmoxAuthenticationHeader(ProxmoxConnectionInfo connectionInfo)
    {
        return new("PVEAPIToken", $"{connectionInfo.User}@{connectionInfo.Realm}!{connectionInfo.TokenId}={connectionInfo.Token}");
    }
}