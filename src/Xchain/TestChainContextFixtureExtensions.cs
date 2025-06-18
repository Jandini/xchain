using System.Runtime.CompilerServices;
using Xunit;

namespace Xchain;

public static class TestChainContextFixtureExtensions
{
    /// <summary>
    /// Skips the test if an exception of type <typeparamref name="TException"/> exists in the test chain.
    /// </summary>
    /// <typeparam name="TException">The type of exception to check for.</typeparam>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="reason">Optional skip reason.</param>
    public static void SkipIf<TException>(this TestChainContextFixture fixture, string? reason = null) where TException : Exception =>
        Skip.If(fixture.Errors.Any(ex => ex is TException), reason);

    /// <summary>
    /// Executes a test step and captures any exception into the chain's error stack.
    /// </summary>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="linkAction">The test logic to execute.</param>
    /// <param name="callerName">Automatically filled with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically filled with the source file path.</param>
    /// <param name="callerLineNumber">Automatically filled with the line number of the call.</param>
    public static void Link(this TestChainContextFixture fixture, Action<TestChainOutput> linkAction,
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
    /// Executes the provided test step and captures any exception in the chain's error stack.
    /// </summary>
    /// <typeparam name="TResult">The return type of the test step.</typeparam>
    /// <param name="fixture">The test chain fixture that holds shared output and error tracking.</param>
    /// <param name="linkAction">The test step to execute, using the fixture's output.</param>
    /// <param name="callerName">Automatically populated with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically populated with the source file path of the call.</param>
    /// <param name="callerLineNumber">Automatically populated with the line number of the call.</param>
    /// <returns>The result returned by <paramref name="linkAction"/>.</returns>
    /// <exception cref="TestChainException">
    /// Thrown when <paramref name="linkAction"/> throws, wrapping the original exception with context.
    /// </exception>
    public static TResult Link<TResult>(this TestChainContextFixture fixture, Func<TestChainOutput, TResult> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            return linkAction(fixture.Output);
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
    public static void LinkUnless<TException>(this TestChainContextFixture fixture, Action<TestChainOutput> linkAction,
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
    /// Executes the provided test step unless an exception of type <typeparamref name="TException"/>
    /// already exists in the test chain's error stack. If such an exception is found, the test is skipped.
    /// </summary>
    /// <typeparam name="TException">The type of exception to check for in the error stack.</typeparam>
    /// <typeparam name="TResult">The return type of the test step.</typeparam>
    /// <param name="fixture">The test chain fixture that holds shared output and error tracking.</param>
    /// <param name="linkAction">The test step to execute, using the fixture's output.</param>
    /// <param name="callerName">Automatically populated with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically populated with the source file path of the call.</param>
    /// <param name="callerLineNumber">Automatically populated with the line number of the call.</param>
    /// <returns>The result returned by <paramref name="linkAction"/> if executed.</returns>
    /// <exception cref="TestChainException">
    /// Thrown when <paramref name="linkAction"/> throws, wrapping the original exception with context.
    /// </exception>
    public static TResult LinkUnless<TException, TResult>(this TestChainContextFixture fixture, Func<TestChainOutput, TResult> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
    {
        try
        {
            var exception = fixture.Errors.FirstOrDefault(ex => ex.InnerException is TException);
            Skip.If(exception is not null, exception?.Message);
            return linkAction(fixture.Output);
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
    public static async Task LinkAsync(this TestChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task> linkAction,
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
    /// Executes an asynchronous test step and captures any exception in the chain's error stack.
    /// </summary>
    /// <typeparam name="TResult">The return type of the asynchronous test step.</typeparam>
    /// <param name="fixture">The test chain fixture that holds shared output and error tracking.</param>
    /// <param name="linkAction">
    /// An asynchronous delegate that receives the fixture's output and a cancellation token, and returns a result.
    /// </param>
    /// <param name="timeOut">An optional timeout after which the operation will be canceled. Defaults to no timeout.</param>
    /// <param name="callerName">Automatically populated with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically populated with the source file path of the call.</param>
    /// <param name="callerLineNumber">Automatically populated with the line number of the call.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of <paramref name="linkAction"/>.</returns>
    /// <exception cref="TestChainException">
    /// Thrown when <paramref name="linkAction"/> throws, wrapping the original exception with context.
    /// </exception>
    public static async Task<TResult> LinkAsync<TResult>(this TestChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task<TResult>> linkAction,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
            return await linkAction(fixture.Output, cts.Token);
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
    public static async Task LinkUnlessAsync<TException>(this TestChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task> linkAction,
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
            await linkAction(fixture.Output, cts.Token);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }


    /// <summary>
    /// Executes an asynchronous test step unless an exception of type <typeparamref name="TException"/>
    /// already exists in the test chain's error stack. If such an exception is found, the test is skipped.
    /// </summary>
    /// <typeparam name="TException">The type of exception to check for in the error stack.</typeparam>
    /// <typeparam name="TResult">The return type of the asynchronous test step.</typeparam>
    /// <param name="fixture">The test chain fixture that holds shared output and error tracking.</param>
    /// <param name="linkAction">
    /// An asynchronous delegate that receives the fixture's output and a cancellation token, and returns a result.
    /// </param>
    /// <param name="timeOut">An optional timeout after which the operation will be canceled. Defaults to no timeout.</param>
    /// <param name="callerName">Automatically populated with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically populated with the source file path of the call.</param>
    /// <param name="callerLineNumber">Automatically populated with the line number of the call.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of <paramref name="linkAction"/> if executed.</returns>
    /// <exception cref="TestChainException">
    /// Thrown when <paramref name="linkAction"/> throws, wrapping the original exception with context.
    /// </exception>
    public static async Task<TResult> LinkUnlessAsync<TException, TResult>(this TestChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task<TResult>> linkAction,
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
            return await linkAction(fixture.Output, cts.Token);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }
}
