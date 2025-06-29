namespace Xchain;

/// <summary>
/// Provides shared state and error tracking for a single test class.
/// Used with <see cref="IClassFixture{TFixture}"/> to coordinate output and exceptions between chained tests.
/// </summary>
public class TestChainContextFixture
{
    /// <summary>
    /// Shared output storage for the current test class.
    /// Allows tests to share data using <see cref="TestChainOutput"/>.
    /// </summary>
    public virtual TestChainOutput Output { get; } = [];

    /// <summary>
    /// Shared exception tracking for the current test class.
    /// Stores any captured exceptions from executed chain steps.
    /// </summary>
    public virtual TestChainErrors Errors { get; } = [];
}
