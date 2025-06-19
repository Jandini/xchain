using System.Runtime.CompilerServices;
using Xunit;

namespace Xchain
{
    public static class CollectionChainContextFixtureExtensions
    {
        public static void LinkWithCollection<TCollection, TOutput>(this CollectionChainContextFixture fixture, TestOutput<TCollection, TOutput> output, Action<TestChainOutput> linkAction, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
            => fixture.LinkWithCollection<TCollection>(output.Key, linkAction, callerName, callerFilePath, callerLineNumber);

        public static void LinkWithCollection<TCollection>(this CollectionChainContextFixture fixture, string outputKey, Action<TestChainOutput> linkAction,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
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


        public static TOutput LinkWithCollection<TCollection, TOutput>(this CollectionChainContextFixture fixture, TestOutput<TCollection, TOutput> output, Func<TestChainOutput, TOutput> linkAction, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
            => fixture.LinkWithCollection<TCollection, TOutput>(output.Key, linkAction, callerName, callerFilePath, callerLineNumber);

        public static TResult LinkWithCollection<TCollection, TResult>(this CollectionChainContextFixture fixture, string outputKey, Func<TestChainOutput, TResult> linkAction,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
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


        public static async Task LinkWithCollectionAsync<TCollection, TOutput>(this CollectionChainContextFixture fixture, TestOutput<TCollection, TOutput> output, Func<TestChainOutput, CancellationToken, Task> linkAction, TimeSpan timeOut = default, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
            => await fixture.LinkWithCollectionAsync<TCollection, TOutput>(output.Key, linkAction, timeOut, callerName, callerFilePath, callerLineNumber);
        
        public static async Task LinkWithCollectionAsync<TCollection, TOutput>(this CollectionChainContextFixture fixture, string outputKey, Func<TestChainOutput, CancellationToken, Task> linkAction, TimeSpan timeOut = default,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
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



        public static async Task<TOutput> LinkWithCollectionAsync<TCollection, TOutput>(this CollectionChainContextFixture fixture, TestOutput<TCollection, TOutput> output, Func<TestChainOutput, CancellationToken, Task<TOutput>> linkAction, TimeSpan timeOut = default, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
            => await fixture.LinkWithCollectionAsync<TCollection, TOutput>(output.Key, linkAction, timeOut, callerName, callerFilePath, callerLineNumber);

        public static async Task<TOutput> LinkWithCollectionAsync<TCollection, TOutput>(this CollectionChainContextFixture fixture, string outputKey, Func<TestChainOutput, CancellationToken, Task<TOutput>> linkAction, TimeSpan timeOut = default,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
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
}


