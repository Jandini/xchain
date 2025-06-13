using Xunit;

namespace Xchain;

public class ChainTheoryAttribute : SkippableTheoryAttribute
{
    public int Link { get; set; } = 0;
    public int Pad { get; set; } = 0;
    public string Flow { get; set; } = string.Empty;
    public string Name
    {
        get => DisplayName;
        set => DisplayName = Link == 0 ? value : $"#{Link.ToString().PadLeft(Pad, '0')} | {(string.IsNullOrEmpty(Flow) ? "" : Flow + " | ")}{value}";
    }
}
