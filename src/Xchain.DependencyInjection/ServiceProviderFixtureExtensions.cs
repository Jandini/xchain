using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Xchain.DependencyInjection;

public static class ServiceProviderFixtureExtensions
{
    public static void Link(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Action<IServiceProvider, TestChainOutput> action,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            using var scope = fixture.Services.CreateScope();
            action(scope.ServiceProvider, chain.Output);
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    public static TResult Link<TResult>(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Func<IServiceProvider, TestChainOutput, TResult> func,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            using var scope = fixture.Services.CreateScope();
            return func(scope.ServiceProvider, chain.Output);
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    public static async Task LinkAsync(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Func<IServiceProvider, TestChainOutput, CancellationToken, Task> action,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            await using var scope = fixture.Services.CreateAsyncScope();
            using var cts = timeOut != default
                ? new CancellationTokenSource(timeOut)
                : new CancellationTokenSource();
            await action(scope.ServiceProvider, chain.Output, cts.Token);
        }
        catch (OperationCanceledException ex) when (timeOut != default)
        {
            var tex = new TimeoutException($"The {callerName} timed out after {timeOut}.", ex);
            chain.Errors.Push(new TestChainException(tex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw tex;
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    public static async Task<TResult> LinkAsync<TResult>(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Func<IServiceProvider, TestChainOutput, CancellationToken, Task<TResult>> func,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            await using var scope = fixture.Services.CreateAsyncScope();
            using var cts = timeOut != default
                ? new CancellationTokenSource(timeOut)
                : new CancellationTokenSource();
            return await func(scope.ServiceProvider, chain.Output, cts.Token);
        }
        catch (OperationCanceledException ex) when (timeOut != default)
        {
            var tex = new TimeoutException($"The {callerName} timed out after {timeOut}.", ex);
            chain.Errors.Push(new TestChainException(tex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw tex;
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    public static void LinkUnless<TException>(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Action<IServiceProvider, TestChainOutput> action,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
    {
        try
        {
            var existing = chain.Errors.FirstOrDefault(ex => ex.InnerException is TException);
            Skip.If(existing is not null, existing?.Message);
            using var scope = fixture.Services.CreateScope();
            action(scope.ServiceProvider, chain.Output);
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    public static TResult LinkUnless<TException, TResult>(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Func<IServiceProvider, TestChainOutput, TResult> func,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
    {
        try
        {
            var existing = chain.Errors.FirstOrDefault(ex => ex.InnerException is TException);
            Skip.If(existing is not null, existing?.Message);
            using var scope = fixture.Services.CreateScope();
            return func(scope.ServiceProvider, chain.Output);
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    public static async Task LinkUnlessAsync<TException>(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Func<IServiceProvider, TestChainOutput, CancellationToken, Task> action,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
    {
        try
        {
            var existing = chain.Errors.FirstOrDefault(ex => ex.InnerException is TException);
            Skip.If(existing is not null, existing?.Message);
            await using var scope = fixture.Services.CreateAsyncScope();
            using var cts = timeOut != default
                ? new CancellationTokenSource(timeOut)
                : new CancellationTokenSource();
            await action(scope.ServiceProvider, chain.Output, cts.Token);
        }
        catch (OperationCanceledException ex) when (timeOut != default)
        {
            var tex = new TimeoutException($"The {callerName} timed out after {timeOut}.", ex);
            chain.Errors.Push(new TestChainException(tex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw tex;
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    public static async Task<TResult> LinkUnlessAsync<TException, TResult>(
        this IServiceProviderFixture fixture,
        TestChainContextFixture chain,
        Func<IServiceProvider, TestChainOutput, CancellationToken, Task<TResult>> func,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
    {
        try
        {
            var existing = chain.Errors.FirstOrDefault(ex => ex.InnerException is TException);
            Skip.If(existing is not null, existing?.Message);
            await using var scope = fixture.Services.CreateAsyncScope();
            using var cts = timeOut != default
                ? new CancellationTokenSource(timeOut)
                : new CancellationTokenSource();
            return await func(scope.ServiceProvider, chain.Output, cts.Token);
        }
        catch (OperationCanceledException ex) when (timeOut != default)
        {
            var tex = new TimeoutException($"The {callerName} timed out after {timeOut}.", ex);
            chain.Errors.Push(new TestChainException(tex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw tex;
        }
        catch (Exception ex)
        {
            chain.Errors.Push(new TestChainException(ex, chain.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }
}
