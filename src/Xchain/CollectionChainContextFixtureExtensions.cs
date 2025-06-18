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


        public static async Task LinkWithCollectionAsync<TCollection, TOutput>(this CollectionChainContextFixture fixture, TestOutput<TCollection, TOutput> output, Func<TestChainOutput, CancellationToken, Task> linkAction, TimeSpan timeOut = default,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            try
            {
                Skip.IfNot(output.ContainsKey(), $"The expected output {output.Key} was not provided by {typeof(TCollection)}.");
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





        //    public static async Task<TResult> LinkCollectionAsync<TResult>(this CollectionChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task<TResult>> linkAction,
        //        TimeSpan timeOut = default,
        //        [CallerMemberName] string callerName = "",
        //        [CallerFilePath] string callerFilePath = "",
        //        [CallerLineNumber] int callerLineNumber = -1)
        //    {
        //        try
        //        {
        //            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
        //            return await linkAction(fixture.Output, cts.Token);
        //        }
        //        catch (Exception ex)
        //        {
        //            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
        //            throw;
        //        }
        //    }


        //    public static async Task LinkCollectionUnlessAsync<TException>(this CollectionChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task> linkAction,
        //        TimeSpan timeOut = default,
        //        [CallerMemberName] string callerName = "",
        //        [CallerFilePath] string callerFilePath = "",
        //        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
        //    {
        //        try
        //        {
        //            var exception = fixture.Errors.FirstOrDefault(ex => ex.InnerException is TException);
        //            Skip.If(exception is not null, exception?.Message);
        //            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
        //            await linkAction(fixture.Output, cts.Token);
        //        }
        //        catch (Exception ex)
        //        {
        //            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
        //            throw;
        //        }
        //    }


        //    public static async Task<TResult> LinkCollectionUnlessAsync<TException, TResult>(this CollectionChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task<TResult>> linkAction,
        //        TimeSpan timeOut = default,
        //        [CallerMemberName] string callerName = "",
        //        [CallerFilePath] string callerFilePath = "",
        //        [CallerLineNumber] int callerLineNumber = -1) where TException : Exception
        //    {
        //        try
        //        {
        //            var exception = fixture.Errors.FirstOrDefault(ex => ex.InnerException is TException);
        //            Skip.If(exception is not null, exception?.Message);
        //            using CancellationTokenSource cts = timeOut != default ? new(timeOut) : new();
        //            return await linkAction(fixture.Output, cts.Token);
        //        }
        //        catch (Exception ex)
        //        {
        //            fixture.Errors.Push(new TestChainException(ex, fixture.Errors, callerName, callerFilePath, callerLineNumber));
        //            throw;
        //        }
        //    }
        //}
    }
}


