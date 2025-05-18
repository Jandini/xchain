using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain
{
    public class LinkOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var sorted = testCases.Select(tc => new
                {
                    TestCase = tc,
                    Order = tc.TestMethod.Method.GetCustomAttributes(typeof(LinkAttribute).AssemblyQualifiedName)
                        .FirstOrDefault()?.GetNamedArgument<int>(nameof(LinkAttribute.Order)) ?? 0
                })
                .OrderBy(x => x.Order)
                .Select(x => x.TestCase);

            return sorted;
        }
    }
}
