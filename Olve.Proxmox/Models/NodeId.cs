namespace Olve.Proxmox.Models;

public readonly record struct NodeId(string Value);

/*
public class StartVMOperation : IAsyncOperation<StartVMOperation.Request, StartVMOperation.Response>
{
    public record Request(ProxmoxConnectionInfo ConnectionInfo, VMIdentifier VmIdentifier);
    public record Response(ProxmoxJobIdentifier JobIdentifier);

    public Task<Result<Response>> ExecuteAsync(Request request, CancellationToken ct = default)
    {

    }
}
*/