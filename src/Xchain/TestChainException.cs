namespace Xchain;

/// <summary>
/// A custom exception wrapper that decorates test failures or skips in a human-readable format.
/// </summary>
/// <remarks>
/// This exception is used internally to wrap exceptions thrown during a test step and annotate them with:
/// - Caller method name
/// - Source file and line number
/// - Distinction between failures and skips
/// - Summary of original and inner exceptions
///
/// If any prior test in the chain has failed, the display changes from ❌ (failed) to ⚠️ (skipped),
/// making logs and output more informative.
/// </remarks>
/// <param name="ex">The original exception that was thrown.</param>
/// <param name="errors">The error stack tracking previously thrown exceptions in the chain.</param>
/// <param name="callerName">The method where the failure occurred.</param>
/// <param name="callerFilePath">The source file path of the method.</param>
/// <param name="callerLineNumber">The source code line where the failure was raised.</param>
internal class TestChainException(
    Exception ex,
    TestChainErrors errors,
    string callerName,
    string callerFilePath,
    int callerLineNumber
) : Exception(
    $"{(errors.Count > 0 ? "⚠️" : "❌")} {callerName} {(errors.Count > 0 ? "skipped" : "failed")}" +
    $"{(errors.Count > 0 ? "." : " in " + Path.GetFileName(callerFilePath) + " line " + callerLineNumber)}" +
    $"\n{ex.Message}" +
    (ex.InnerException != null ? $"\n{ex.InnerException.Message}" : ""),
    ex)
{
}
