using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Olve.Proxmox;
using Olve.Proxmox.Operations;

var serviceCollection = new ServiceCollection();

serviceCollection.AddProxmoxServices();

var serviceProvider = serviceCollection.BuildServiceProvider();


var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var apiKeyFile = Path.Combine(assemblyDir, ".proxmox-api-key");

var apiKey = File.ReadAllText(apiKeyFile).Trim();

ProxmoxConnectionInfo connectionInfo = new(
    Host: "fortress.lan",
    Port: 8006,
    User: "olve.proxmox",
    Realm: "pam",
    TokenId: "olve.proxmox",
    Token: apiKey);

var operation = serviceProvider.GetRequiredService<ListPCIMappingsOperation>();
ListPCIMappingsOperation.Request request = new(connectionInfo);
var result = await operation.ExecuteAsync(request);

if (result.TryPickProblems(out var problems, out var response))
{
    foreach (var problem in problems)
    {
        var debugString = problem.ToDebugString();
        Console.WriteLine(debugString);
    }

    return 1;
}

foreach (var mapping in response.PCIMappings)
{
    Console.WriteLine(mapping);
}

return 0;
