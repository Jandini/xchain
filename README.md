# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

Xchain extends xUnit with a fluent mechanism to chain tests, pass data between them, and skip dependent tests if earlier ones fail. It improves test readability and flow control when writing integration or system-level tests that have interdependencies.


### Key Features

- **Custom test ordering** via `[Link(order)]`
- **Shared output** across chained tests using `TestChainFixture`
- **Failure-aware execution** — skip dependent tests if prior ones failed
- **Automatic exception capture** for reporting/debugging



### 🧪 Chained Test Execution with Xchain

This test collection demonstrates how Xchain tracks failures and skips dependent tests in a chain.

```csharp
[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest(TestChainFixture chain) : IClassFixture<TestChainFixture>
{
    [ChainFact, Link(3)]
    public void Test1() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException(); // Skipped due to previous Exception
    });

    [ChainFact, Link(2)]
    public void Test2() => chain.LinkUnless<NotImplementedException>((output) =>
    {
        var sleep = output.Get<int>("Sleep");
        Thread.Sleep(sleep); // Succeeds, runs before NotImplementedException happens
    });

    [ChainFact, Link(1)]
    public void Test3() => chain.Link((output) =>
    {
        var sleep = 100;
        Thread.Sleep(sleep);
        output["Sleep"] = sleep * 2;
        throw new TimeoutException(); // Root failure
    });

    [ChainFact, Link(4)]
    public void Test4() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException(); // Skipped due to Test1 being skipped
    });

    [ChainFact, Link(5)]
    public void Test5() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException(); // Skipped due to Test4 being skipped
    });
}
```

### 🔎 Output

```text
Xchain.Tests.ChainTest.Test3              // ❌ Test3 failed (root failure)
  Source: ChainTest.cs line 22
  Duration: 104 ms
  Message:                                   
    System.TimeoutException : The operation has timed out.    

Xchain.Tests.ChainTest.Test2              // ✅ Test2 ran successfully
  Source: ChainTest.cs line 14
  Duration: 209 ms                                       

Xchain.Tests.ChainTest.Test1              // ⚠️ Skipped due to Test3 failure
  Source: ChainTest.cs line 7
  Duration: 1 ms
  Message:
    Test3 failed in ChainTest.cs line 22.
    The operation has timed out.                          

Xchain.Tests.ChainTest.Test4              // ⚠️ Skipped due to Test1 being skipped
  Source: ChainTest.cs line 32
  Duration: 1 ms
  Message:
    Test1 skipped in ChainTest.cs line 7.
    Test3 failed in ChainTest.cs line 22.
    The operation has timed out.                          

Xchain.Tests.ChainTest.Test5              // ⚠️ Skipped due to Test4 → Test1 → Test3 chain
  Source: ChainTest.cs line 38
  Duration: 1 ms
  Message:
    Test4 skipped in ChainTest.cs line 32.
    Test1 skipped in ChainTest.cs line 7.
    Test3 failed in ChainTest.cs line 22.
    The operation has timed out.                          
```

### 📘 Summary

- **Test3** fails with a `TimeoutException` — it is the root failure.
- **Test2** succeeds — it only skips on `NotImplementedException`, which did not occur.
- **Test1** is skipped because it depends on any `Exception`, and Test3 failed.
- **Test4** is skipped because Test1 was skipped.
- **Test5** is skipped because Test4 was skipped — demonstrating deep chaining.




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
public class ChainTest(TestChainFixture chain) : IClassFixture<TestChainFixture>
{
    [ChainFact, Link(3)]
    public void Test1() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });


    [ChainFact, Link(2)]
    public void Test2() => chain.LinkUnless<NotImplementedException>((output) =>
    {
        var sleep = output.Get<int>("Sleep");
        Thread.Sleep(sleep);
    });
    

    [ChainFact, Link(1)]
    public void Test3() => chain.Link((output) =>
    {
        var sleep = 1000;
        Thread.Sleep(sleep);
        output["Sleep"] = sleep * 2;

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

Powered by [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact)