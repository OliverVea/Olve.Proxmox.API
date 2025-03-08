namespace Olve.Proxmox;

public readonly record struct ProxmoxConnectionInfo(string Host, int Port, string User, string Realm, string TokenId, string Token);