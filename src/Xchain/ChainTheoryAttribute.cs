using Xunit;

namespace Xchain;

/// <summary>
/// A custom theory attribute for chained and ordered test cases that use parameterized data.
/// Inherits from <see cref="SkippableTheoryAttribute"/> to allow conditional skipping.
///
/// Adds ordering and display metadata, enhancing test structure and output readability.
/// </summary>
public class ChainTheoryAttribute : SkippableTheoryAttribute
{
    /// <summary>
    /// Execution order of the test within the chain.
    /// Lower values execute earlier. Default is 0.
    /// </summary>
    public int Link { get; set; } = 0;

    /// <summary>
    /// Optional padding for the Link value (e.g., Pad=2 formats Link=5 as "05").
    /// Aids in aligning test case names visually.
    /// </summary>
    public int Pad { get; set; } = 0;

    /// <summary>
    /// Optional flow label to group tests logically by purpose or scenario.
    /// Included in the test display name for better visual distinction.
    /// </summary>
    public string Flow { get; set; } = string.Empty;

    /// <summary>
    /// Sets the test case’s display name shown in test runners.
    /// Format: "#Link | Flow | Name", omitting Flow if not defined.
    /// </summary>
    public string Name
    {
        get => DisplayName;
        set => DisplayName = Link == 0
            ? value
            : $"#{Link.ToString().PadLeft(Pad, '0')} | {(string.IsNullOrEmpty(Flow) ? "" : Flow + " | ")}{value}";
    }
}
