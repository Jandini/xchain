namespace Xchain;

internal class TestChainException(Exception ex, TestChainErrors errors, string callerName, string callerFilePath, int callerLineNumber) 
    : Exception($"{(errors.Count > 0 ? "⚠️" : "❌")} {callerName} {(errors.Count > 0 ? "skipped" : "failed")} in {Path.GetFileName(callerFilePath)} line {callerLineNumber}.\n{ex.Message}{(ex.InnerException != null ? $"\n{ex.InnerException.Message}" : "")}", ex)
{
}