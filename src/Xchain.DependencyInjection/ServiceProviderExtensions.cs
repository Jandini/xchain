using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace Xchain.DependencyInjection;

internal static class ServiceProviderExtensions
{
    internal static void StartHostedServices(this IServiceProvider provider, IMessageSink sink)
    {
        foreach (var svc in provider.GetServices<IHostedService>())
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            svc.StartAsync(cts.Token).GetAwaiter().GetResult();
            sink.Write($"[xchain] started {svc.GetType().Name}");
        }
    }

    internal static void StopHostedServices(this IServiceProvider provider, IMessageSink sink)
    {
        foreach (var svc in provider.GetServices<IHostedService>())
        {
            try
            {
                svc.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
                sink.Write($"[xchain] stopped {svc.GetType().Name}");
            }
            catch (Exception ex)
            {
                sink.Write($"[xchain] stop error {svc.GetType().Name}: {ex.Message}");
            }
        }
    }

    internal static async Task StopHostedServicesAsync(this IServiceProvider provider, IMessageSink sink)
    {
        foreach (var svc in provider.GetServices<IHostedService>())
        {
            try
            {
                await svc.StopAsync(CancellationToken.None);
                sink.Write($"[xchain] stopped {svc.GetType().Name}");
            }
            catch (Exception ex)
            {
                sink.Write($"[xchain] stop error {svc.GetType().Name}: {ex.Message}");
            }
        }
    }
}
