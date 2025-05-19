using Xunit;
using Xunit.Sdk;

namespace Xchain
{
  //  [XunitTestCaseDiscoverer("Xchain.TestChainDiscoverer", "Xchain")]
    public sealed class ChainFactAttribute : SkippableFactAttribute
    {
        public override string DisplayName { get => base.DisplayName; set => base.DisplayName = value; }
    }
}
