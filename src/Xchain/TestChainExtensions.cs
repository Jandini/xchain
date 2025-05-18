using System.Runtime.CompilerServices;
using Xunit;

namespace Xchain;

/// <summary>
/// Extension methods to enable test chaining, exception tracking, and conditional skipping in xUnit test flows.
/// </summary>
public static class TestChainExtensions
{
    /// <summary>
    /// Skips the test if an exception of type <typeparamref name="T"/> exists in the test chain.
    /// </summary>
    /// <typeparam name="T">The type of exception to check for.</typeparam>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="reason">Optional skip reason.</param>
    public static void SkipIf<T>(this TestChainFixture fixture, string? reason = null) where T : Exception =>
        Skip.If(fixture.Errors.Any(ex => ex is T), reason);

    /// <summary>
    /// Retrieves a value from the test chain output by name or throws if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The expected type of the output value.</typeparam>
    /// <param name="output">The test chain output.</param>
    /// <param name="name">The key name of the output item.</param>
    /// <returns>The value cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="TestChainOutputMissingException">Thrown if the named output is missing.</exception>
    public static T Get<T>(this TestChainOutput output, string name) =>
        output.ContainsKey(name) ? (T)output[name] : throw new TestChainOutputMissingException(name);

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
            fixture.Errors.Push(new TestChainException(ex, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes a test step unless a specific exception type exists in the chain.
    /// </summary>
    /// <typeparam name="T">The exception type to check for in the error stack.</typeparam>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="linkAction">The test logic to execute.</param>
    /// <param name="callerName">Automatically filled with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically filled with the source file path.</param>
    /// <param name="callerLineNumber">Automatically filled with the line number of the call.</param>
    public static void LinkUnless<T>(this TestChainFixture fixture, Action<TestChainOutput> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where T : Exception
    {
        try
        {
            var exception = fixture.Errors.FirstOrDefault(ex => ex.InnerException is T);
            Skip.If(exception is not null, exception?.Message);
            linkAction(fixture.Output);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, callerName, callerFilePath, callerLineNumber));
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
    public static async Task LinkAsync(this TestChainFixture fixture, Func<TestChainOutput, Task> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1)
    {
        try
        {
            await linkAction(fixture.Output);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous test step unless a specific exception type exists in the chain.
    /// </summary>
    /// <typeparam name="T">The exception type to check for in the error stack.</typeparam>
    /// <param name="fixture">The test chain fixture.</param>
    /// <param name="linkAction">An async delegate containing test logic.</param>
    /// <param name="callerName">Automatically filled with the calling method's name.</param>
    /// <param name="callerFilePath">Automatically filled with the source file path.</param>
    /// <param name="callerLineNumber">Automatically filled with the line number of the call.</param>
    public static async Task LinkUnlessAsync<T>(this TestChainFixture fixture, Func<TestChainOutput, Task> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) where T : Exception
    {
        try
        {
            var exception = fixture.Errors.FirstOrDefault(ex => ex.InnerException is T);
            Skip.If(exception is not null, exception?.Message);
            await linkAction(fixture.Output);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }
}
