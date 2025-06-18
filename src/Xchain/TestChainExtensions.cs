using System.Runtime.CompilerServices;
using Xunit;

namespace Xchain;

/// <summary>
/// Extension methods to enable test chaining, exception tracking, and conditional skipping in xUnit test flows.
/// </summary>
public static class TestChainExtensions
{
    /// <summary>
    /// Skips the test if an exception of type <typeparamref name="TException"/> exists in the test chain.
    /// </summary>
    /// <typeparam name="TException">The type of exception to check for.</typeparam>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="reason">Optional skip reason.</param>
    public static void SkipIf<TException>(this TestChainFixture fixture, string? reason = null) where TException : Exception =>
        Skip.If(fixture.Errors.Any(ex => ex is TException), reason);

    /// <summary>
    /// Executes a test step and captures any exception into the chain's error stack.
    /// </summary>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="linkAction">The test logic to execute.</param>
    /// <param name="callerName">Automatically filled with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically filled with the source file path.</param>
    /// <param name="callerLineNumber">Automatically filled with the line number of the call.</param>
    public static void Link(this TestChainFixture fixture, Action<TestChainOutput> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            linkAction(fixture.Output);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes a test step unless a specific exception type exists in the chain.
    /// </summary>
    /// <typeparam name="TException">The exception type to check for in the error stack.</typeparam>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="linkAction">The test logic to execute.</param>
    /// <param name="callerName">Automatically filled with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically filled with the source file path.</param>
    /// <param name="callerLineNumber">Automatically filled with the line number of the call.</param>
    public static void LinkUnless<TException>(this TestChainFixture fixture, Action<TestChainOutput> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
    {
        try
        {
            var exception = fixture.Errors.FirstOrDefault(ex => ex.InnerException is TException);
            Skip.If(exception is not null, exception?.Message);
            linkAction(fixture.Output);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous test step and captures any exception into the chain's error stack.
    /// </summary>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="linkAction">An async delegate containing test logic.</param>
    /// <param name="callerName">Automatically filled with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically filled with the source file path.</param>
    /// <param name="callerLineNumber">Automatically filled with the line number of the call.</param>
    public static async Task LinkAsync(this TestChainFixture fixture, Func<TestChainOutput, CancellationToken, Task> linkAction,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
            await linkAction(fixture.Output, cts.Token);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous test step unless a specific exception type exists in the chain.
    /// </summary>
    /// <typeparam name="TException">The exception type to check for in the error stack.</typeparam>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="linkAction">An async delegate containing test logic.</param>
    /// <param name="callerName">Automatically filled with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically filled with the source file path.</param>
    /// <param name="callerLineNumber">Automatically filled with the line number of the call.</param>
    public static async Task LinkUnlessAsync<TException>(this TestChainFixture fixture, Func<TestChainOutput, CancellationToken, Task> linkAction,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
    {
        try
        {
            var exception = fixture.Errors.FirstOrDefault(ex => ex.InnerException is TException);
            Skip.If(exception is not null, exception?.Message);
            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
            await linkAction(fixture.Output, cts.Token  );
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }
}
