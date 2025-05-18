namespace Xchain;

internal class TestChainException(Exception ex, string callerName, string callerFilePath, int callerLineNumber) 
    : Exception($"{callerName} failed in {Path.GetFileName(callerFilePath)} line {callerLineNumber}.\n{ex.Message}", ex)
{
}