using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain
{
    public class TestChainDiscoverer(IMessageSink diagnosticMessageSink) : IXunitTestCaseDiscoverer
    {
        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            diagnosticMessageSink.OnMessage(new DiagnosticMessage("########### hello world"));
            //// Try to get the LinkAttribute on the method
            //var linkAttr = testMethod.Method
            //    .GetCustomAttributes(typeof(LinkAttribute).FullName!)
            //    .FirstOrDefault();

            //var order = linkAttr?.GetConstructorArguments().FirstOrDefault() ?? "–";

            //var displayName = $"# Link Order {order} | {testMethod.Method.Name}";

            //diagnosticMessageSink.OnMessage(new DiagnosticMessage(displayName));

            yield return new XunitTestCase(
                diagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod);
        }
    }
    
    
}
