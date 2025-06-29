using Xunit;

namespace Xchain;

/// <summary>
/// A custom fact attribute for chaining test cases in a specific order.
/// Inherits from <see cref="SkippableFactAttribute"/> to support conditional skipping.
///
/// Adds metadata used by the test orderer and enhances test display names
/// for better readability and logical grouping in test explorers.
/// </summary>
public class ChainFactAttribute : SkippableFactAttribute
{
    /// <summary>
    /// Execution order of the test within the chain.
    /// Lower values execute earlier. Default is 0.
    /// </summary>
    public int Link { get; set; } = 0;

    /// <summary>
    /// Optional padding for the Link value (e.g., Pad=2 formats Link=5 as "05").
    /// Helps align test names visually in logs or test explorers.
    /// </summary>
    public int Pad { get; set; } = 0;

    /// <summary>
    /// Optional flow name used to group related tests under a logical name.
    /// Appears in the test display name to indicate affiliation.
    /// </summary>
    public string Flow { get; set; } = string.Empty;

    /// <summary>
    /// Sets the formatted display name shown in test runners.
    /// Format: "#Link | Flow | Name" — with Flow omitted if not set.
    /// </summary>
    public string Name
    {
        get => DisplayName;
        set => DisplayName = Link == 0
            ? value
            : $"#{Link.ToString().PadLeft(Pad, '0')} | {(string.IsNullOrEmpty(Flow) ? "" : Flow + " | ")}{value}";
    }
}
