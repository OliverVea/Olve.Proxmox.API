namespace Olve.Proxmox;

public static class ProxmoxUrlHelper
{
    public static string BaseUrl(ProxmoxConnectionInfo connectionInfo) => $"https://{connectionInfo.Host}:{connectionInfo.Port}/api2/json";

    public static string ListNodesUrl(ProxmoxConnectionInfo connectionInfo) => BaseUrl(connectionInfo) + "/nodes";

    public static string ListPCIMappings(ProxmoxConnectionInfo connectionInfo) =>
        BaseUrl(connectionInfo) + "/cluster/mapping/pci";
}