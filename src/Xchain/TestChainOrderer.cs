using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xchain;

/// <summary>
/// Custom test case orderer for Xchain that supports ordering test methods
/// based on the 'Link' property in <see cref="ChainFactAttribute"/> or <see cref="ChainTheoryAttribute"/>.
/// </summary>
public class TestChainOrderer : ITestCaseOrderer
{
    /// <summary>
    /// Orders the test cases based on their declared Link value.
    /// If a method lacks a ChainFact or ChainTheory attribute, it receives the default order (int.MaxValue).
    /// </summary>
    /// <typeparam name="TTestCase">The test case type.</typeparam>
    /// <param name="testCases">The collection of test cases to be ordered.</param>
    /// <returns>An ordered sequence of test cases based on the Link value.</returns>
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
        testCases
            .Select(tc =>
            {
                var method = tc.TestMethod.Method;

                // Try to retrieve ChainFact or ChainTheory attributes
                var factAttr = method.GetCustomAttributes(typeof(ChainFactAttribute).AssemblyQualifiedName).FirstOrDefault();
                var theoryAttr = method.GetCustomAttributes(typeof(ChainTheoryAttribute).AssemblyQualifiedName).FirstOrDefault();

                // Extract the Link value; fallback to int.MaxValue if not present
                int order = factAttr?.GetNamedArgument<int>(nameof(ChainFactAttribute.Link))
                            ?? theoryAttr?.GetNamedArgument<int>(nameof(ChainTheoryAttribute.Link))
                            ?? int.MaxValue;

                return new { TestCase = tc, Order = order };
            })
            .OrderBy(x => x.Order) // Order by the extracted Link value
            .Select(x => x.TestCase); // Return the original test cases in the new order
}
