# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

Xchain provides Xunit fixtures to chain tests, share results, and skip tests on failure.


Use `LinkOrderer` to run the test cases in given order. 

```c#
[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest
{
    [Fact, Link(1)]
    public void Test1()
    {
        Thread.Sleep(1000);
    }

    [Fact, Link(3)]
    public void Test2()
    {
        Thread.Sleep(3000);
    }

    [Fact, Link(2)]
    public void Test3()
    {
        Thread.Sleep(2000);
    }
}
```


Use `TestChainFixture` to pass output from one test case to another.

```c#
[TestCaseOrderer("Xchain.LinkOrderer", "Xchain")]
public class ChainTest(TestChainFixture testChain) : IClassFixture<TestChainFixture>
{
    [Fact, Link(3)]
    public void Test1()
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)        
        {
            testChain.Errors.Push(ex);
        }

    }

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
        Thread.Sleep(sleep);
        testChain.Output["Sleep"] = sleep * 2;
    }
}
```

---
Created from [JandaBox](https://github.com/Jandini/JandaBox)
Box icon created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/box)
