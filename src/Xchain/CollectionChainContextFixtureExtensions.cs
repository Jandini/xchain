using System.Runtime.CompilerServices;
using Xunit;

namespace Xchain;

/// <summary>
/// Provides fluent linking APIs for executing test steps across test collections using <see cref="CollectionChainContextFixture"/>.
/// </summary>
/// <remarks>
/// These methods ensure that cross-collection dependencies can be validated and executed safely by checking
/// that a required output key exists before continuing execution.
/// Unlike class-level chaining, this does not inspect or skip based on exceptions — only output presence.
/// </remarks>
public static class CollectionChainContextFixtureExtensions
{
    /// <summary>
    /// Executes a step using shared output from another test collection.
    /// Validates that the required output key exists before executing.
    /// </summary>
    public static void LinkWithCollection<TCollection, TOutput>(
        this CollectionChainContextFixture fixture,
        TestOutput<TCollection, TOutput> output,
        Action<TestChainOutput> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    ) => fixture.LinkWithCollection<TCollection>(output.Key, linkAction, callerName, callerFilePath, callerLineNumber);

    public static void LinkWithCollection<TCollection>(
        this CollectionChainContextFixture fixture,
        string outputKey,
        Action<TestChainOutput> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    )
    {
        try
        {
            Skip.IfNot(fixture.Output.ContainsKey(outputKey), $"The expected output {outputKey} was not provided by {typeof(TCollection)}.");
            linkAction(fixture.Output);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes a step using shared output from another test collection and returns a result.
    /// </summary>
    public static TOutput LinkWithCollection<TCollection, TOutput>(
        this CollectionChainContextFixture fixture,
        TestOutput<TCollection, TOutput> output,
        Func<TestChainOutput, TOutput> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    ) => fixture.LinkWithCollection<TCollection, TOutput>(output.Key, linkAction, callerName, callerFilePath, callerLineNumber);

    public static TResult LinkWithCollection<TCollection, TResult>(
        this CollectionChainContextFixture fixture,
        string outputKey,
        Func<TestChainOutput, TResult> linkAction,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    )
    {
        try
        {
            Skip.IfNot(fixture.Output.ContainsKey(outputKey), $"The expected output {outputKey} was not provided by {typeof(TCollection)}.");
            return linkAction(fixture.Output);
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous step using shared output from another test collection.
    /// </summary>
    public static async Task LinkWithCollectionAsync<TCollection, TOutput>(
        this CollectionChainContextFixture fixture,
        TestOutput<TCollection, TOutput> output,
        Func<TestChainOutput, CancellationToken, Task> linkAction,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    ) => await fixture.LinkWithCollectionAsync<TCollection, TOutput>(output.Key, linkAction, timeOut, callerName, callerFilePath, callerLineNumber);

    public static async Task LinkWithCollectionAsync<TCollection, TOutput>(
        this CollectionChainContextFixture fixture,
        string outputKey,
        Func<TestChainOutput, CancellationToken, Task> linkAction,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    )
    {
        try
        {
            Skip.IfNot(fixture.Output.ContainsKey(outputKey), $"The expected output {outputKey} was not provided by {typeof(TCollection)}.");
            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
            await linkAction(fixture.Output, cts.Token);
        }
        catch (OperationCanceledException ex) when (timeOut != default)
        {
            var tex = new TimeoutException($"The {callerName} timed out after {timeOut}.", ex);
            fixture.Errors.Push(new TestChainException(tex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw tex;
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous step using shared output from another test collection and returns a result.
    /// </summary>
    public static async Task<TOutput> LinkWithCollectionAsync<TCollection, TOutput>(
        this CollectionChainContextFixture fixture,
        TestOutput<TCollection, TOutput> output,
        Func<TestChainOutput, CancellationToken, Task<TOutput>> linkAction,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    ) => await fixture.LinkWithCollectionAsync<TCollection, TOutput>(output.Key, linkAction, timeOut, callerName, callerFilePath, callerLineNumber);

    public static async Task<TOutput> LinkWithCollectionAsync<TCollection, TOutput>(
        this CollectionChainContextFixture fixture,
        string outputKey,
        Func<TestChainOutput, CancellationToken, Task<TOutput>> linkAction,
        TimeSpan timeOut = default,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
    )
    {
        try
        {
            Skip.IfNot(fixture.Output.ContainsKey(outputKey), $"The expected output {outputKey} was not provided by {typeof(TCollection)}.");
            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
            return await linkAction(fixture.Output, cts.Token);
        }
        catch (OperationCanceledException ex) when (timeOut != default)
        {
            var tex = new TimeoutException($"The {callerName} timed out after {timeOut}.", ex);
            fixture.Errors.Push(new TestChainException(tex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw tex;
        }
        catch (Exception ex)
        {
            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
            throw;
        }
    }
}
