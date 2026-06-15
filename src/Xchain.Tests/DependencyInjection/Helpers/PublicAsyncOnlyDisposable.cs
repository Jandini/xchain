namespace Xchain.Tests.DependencyInjection.Helpers;

public sealed class PublicAsyncOnlyDisposable : IAsyncDisposable
{
    public bool Disposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}
