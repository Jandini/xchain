# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

Xchain provides Xunit fixtures to chain tests, share results, and skip tests on failure.


Use `LinkOrderer` to run the test cases in given order. 

```C#
namespace Xchain.Tests;

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

---
Created from [JandaBox](https://github.com/Jandini/JandaBox)
Box icon created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/box)
