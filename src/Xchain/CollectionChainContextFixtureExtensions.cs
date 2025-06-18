using System.Runtime.CompilerServices;
using Xunit;

namespace Xchain
{
    public static class CollectionChainContextFixtureExtensions
    {
        public static void LinkCollection<T>(this CollectionChainContextFixture fixture, Action<TestChainOutput> linkAction,
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

        public static void LinkCollectionUnless<TException>(this CollectionChainContextFixture fixture, Action<TestChainOutput> linkAction,
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


        public static TResult LinkCollectionUnless<TException, TResult>(this CollectionChainContextFixture fixture, Func<TestChainOutput, TResult> linkAction,
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


        public static async Task LinkCollectionAsync(this CollectionChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task> linkAction,
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



        public static async Task<TResult> LinkCollectionAsync<TResult>(this CollectionChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task<TResult>> linkAction,
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


        public static async Task LinkCollectionUnlessAsync<TException>(this CollectionChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task> linkAction,
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


        public static async Task<TResult> LinkCollectionUnlessAsync<TException, TResult>(this CollectionChainContextFixture fixture, Func<TestChainOutput, CancellationToken, Task<TResult>> linkAction,
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
}

