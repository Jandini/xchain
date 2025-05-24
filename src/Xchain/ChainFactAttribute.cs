using Xunit;

namespace Xchain;

public sealed class ChainFactAttribute : SkippableFactAttribute
{
    public int Link { get; set; } = 0;
    public int Pad { get; set; } = 0;

    public string Name
    {
        get => DisplayName;
        set => DisplayName = Link == 0 ? value : $"#{Link.ToString().PadLeft(Pad, '0')} | {value}";
    }
}
