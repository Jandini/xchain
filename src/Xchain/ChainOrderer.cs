using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain;

public class ChainOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase 
        => testCases.Select(tc =>
        {
            var method = tc.TestMethod.Method;
            var factAttr = method.GetCustomAttributes(typeof(ChainFactAttribute).AssemblyQualifiedName).FirstOrDefault();
            var theoryAttr = method.GetCustomAttributes(typeof(ChainTheoryAttribute).AssemblyQualifiedName).FirstOrDefault();

            int order = factAttr?.GetNamedArgument<int>(nameof(ChainFactAttribute.Order))
                        ?? theoryAttr?.GetNamedArgument<int>(nameof(ChainTheoryAttribute.Order))
                        ?? 0;

            return new { TestCase = tc, Order = order };
        })
        .OrderBy(x => x.Order)
        .Select(x => x.TestCase);
}
