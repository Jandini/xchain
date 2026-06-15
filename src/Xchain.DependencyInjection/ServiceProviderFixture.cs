using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xchain.DependencyInjection.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Xchain.DependencyInjection;

public sealed class ServiceProviderFixture(IMessageSink messageSink) : IServiceProviderFixture, IAsyncLifetime, IAsyncDisposable
{
    private ServiceProvider? _provider;
    private readonly object _lock = new();

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    public IServiceProvider Services => _provider
        ?? throw new InvalidOperationException("Call Build() before accessing Services.");

    /// <summary>
    /// Builds the <see cref="IServiceProvider"/> on the first call and returns the cached instance on
    /// subsequent calls.
    /// </summary>
    public IServiceProvider Build(Action<IServiceCollection, IConfiguration>? configure = null)
    {
        if (_provider is not null) return _provider;
        lock (_lock)
        {
            if (_provider is not null) return _provider;
            var config = BuildConfiguration();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddLogging(l => l.AddXchainMessageSink(messageSink));
            configure?.Invoke(services, config);
            _provider = services.BuildServiceProvider();
            _provider.StartHostedServices(messageSink);
            return _provider;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_provider is null) return;
        await _provider.StopHostedServicesAsync(messageSink);
        await _provider.DisposeAsync();
        _provider = null;
    }

    internal static IConfiguration BuildConfiguration()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
               ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
               ?? "Test";
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
