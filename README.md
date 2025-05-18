# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

Xchain extends xUnit with a fluent mechanism to chain tests, pass data between them, and skip dependent tests if earlier ones fail. It improves test readability and flow control when writing integration or system-level tests that have interdependencies.


### Key Features

- **Custom test ordering** via `[Link(order)]`
- **Shared output** across chained tests using `TestChainFixture`
- **Failure-aware execution** — skip dependent tests if prior ones failed
- **Automatic exception capture** for reporting/debugging


### 🧪 Example 1: Ordered Execution with `[Link]`

This demonstrates using the `LinkOrderer` to control test run sequence based on the provided order value.

```csharp
[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest
{
    [Fact, Link(1)] public void Test1() => Thread.Sleep(1000);
    [Fact, Link(3)] public void Test2() => Thread.Sleep(3000);
    [Fact, Link(2)] public void Test3() => Thread.Sleep(2000);
}
```

➡️ **What it shows**: Tests will run in the order 1 → 3 → 2 based on their `[Link(x)]` values, not alphabetically or by name.


### 🧪 Example 2: Sharing Data with `TestChainFixture`

This example shows how test output values and errors can be shared across test methods using the fixture.

```csharp
[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest(TestChainFixture testChain) : IClassFixture<TestChainFixture>
{
    [Fact, Link(3)]
    public void Test1() => testChain.Errors.Push(new NotImplementedException());

    [Fact, Link(2)]
    public void Test2()
    {
        var sleep = (int)testChain.Output["Sleep"];
        Thread.Sleep(sleep);
    }

    [Fact, Link(1)]
    public void Test3()
    {
        var sleep = 1000;
        testChain.Output["Sleep"] = sleep * 2;
        Thread.Sleep(sleep);
    }
}
```

➡️ **What it shows**:  
- `Test3` sets shared output.  
- `Test2` reads that output and uses it.  
- `Test1` pushes an error manually to simulate a failure.

---

### 🧪 Example 3: Skipping on Error with `Link` and `LinkUnless`

This uses the fluent API to automatically skip tests based on prior failures.

```csharp
[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest(TestChainFixture testChain) : IClassFixture<TestChainFixture>
{
    [ChainFact, Link(3)]
    public void Test1() =>
        testChain.LinkUnless<Exception>(output => throw new NotImplementedException());

    [ChainFact, Link(2)]
    public void Test2() =>
        testChain.LinkUnless<NotImplementedException>(output =>
        {
            var sleep = output.Get<int>("Sleep");
            Thread.Sleep(sleep);
        });

    [ChainFact, Link(1)]
    public void Test3() =>
        testChain.Link(output =>
        {
            var sleep = 1000;
            output["Sleep"] = sleep * 2;
            Thread.Sleep(sleep);
            throw new TimeoutException();
        });
}
```

➡️ **What it shows**:
- `LinkUnless<T>` skips the test if a matching exception occurred earlier.
- `Link` captures and records exceptions for dependent tests to react to.


---
Created from [JandaBox](https://github.com/Jandini/JandaBox)
Box icon created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/box)
