using Xunit;

namespace Xchain;

public sealed class ChainFactAttribute : SkippableFactAttribute
{
    public int Order { get; set; } = 0;
    public int Pad { get; set; } = 0;

    public string Name
    {
        get => DisplayName;
        set => DisplayName = Order == 0 ? value : $"#{Order.ToString().PadLeft(Pad, '0')} | {value}";
    }
}
