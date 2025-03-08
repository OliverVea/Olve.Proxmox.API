using Microsoft.Extensions.DependencyInjection;
using Olve.Proxmox.Operations;
using Olve.Proxmox.Resilience;
using Polly;

namespace Olve.Proxmox;

public static class ServiceExtensions
{
    public static IServiceCollection AddProxmoxServices(this IServiceCollection services)
    {
        services.AddSingleton<CheckProxmoxConnectionOperation>();
        services.AddSingleton<ListPCIMappingsOperation>();

        services.AddSingleton<ProxmoxHttpService>();

        services.AddHttpClient();
        services.AddResiliencePipeline(ProxmoxHttpService.DefaultResiliencePipelineKey, builder =>
        {
            builder.AddDefaultResiliencePolicies();
        });

        return services;
    }
}