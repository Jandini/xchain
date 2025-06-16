# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

Xchain extends xUnit with a fluent mechanism to **chain tests**, **pass data**, and **skip dependent tests** if previous ones fail — ideal for integration or system tests with interdependencies.


## Features

- **Chained execution**: Tests can conditionally run based on previous outcomes.
- **Shared output state**: Tests exchange data via `TestChainFixture`.
- **Skips on failure**: Later tests are skipped if earlier ones fail.
- **Custom ordering**: Tests are run in a defined sequence using `Link`.
- **Named tests**: Set display name with `Name`, auto-prepended with `# Link`.
- **Zero-padded sorting**: Use `Pad` to ensure consistent numeric display alignment.
- **Custom metadata**: Add test traits via simple attribute classes.


## Example: Chained Execution with Display Names

```csharp
[TestCaseOrderer("Xchain.ChainOrderer", "Xchain")]
public class ChainTest(TestChainFixture chain) : IClassFixture<TestChainFixture>
{
    [ChainFact(Link = 3, Name = "Throw Exception")]
    public void Test1() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });

    [ChainFact(Link = 2, Name = "Sleep 2 seconds")]
    public async Task Test2() => await chain.LinkUnlessAsync<NotImplementedException>(async (output, cancellationToken) =>
    {
        var sleep = output.Get<int>("Sleep");
        await Task.Delay(sleep, cancellationToken);
    });

    [ChainFact(Link = 1, Name = "Sleep 1 second", Pad = 2)]
    [ChainTag(Owner = "Kethoneinuo", Category = "Important", Color = "Black")]
    public async Task Test3() => await chain.LinkAsync(async (output, cancellationToken) =>
    {
        const int sleep = 1000;
        output["Sleep"] = sleep * 2;
        await Task.Delay(sleep, cancellationToken);
    }, TimeSpan.FromMilliseconds(100));

    [ChainFact(Link = 4, Name = "Fails again")]
    public void Test4() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });

    [ChainFact(Link = 5, Name = "Yet another fail")]
    public void Test5() => chain.LinkUnless<Exception>((output) =>
    {
        throw new NotImplementedException();
    });
}
```

Each test is displayed as:

```
#01 | Sleep 1 second
#2 | Sleep 2 seconds
#3 | Throw Exception
#4 | Fails again
#5 | Yet another fail
```

If `Pad = 2`, it ensures alignment even when Link goes beyond 9 (e.g., `#01`, `#10`, `#15`).


## Fluent Chaining Methods

- `Link` — executes and captures exceptions  
- `LinkUnless<TException>` — skips test if exception `TException` was previously thrown  
- `LinkAsync` / `LinkUnlessAsync<TException>` — async equivalents  


## Sharing Data Across Tests

Xchain uses a `TestChainFixture` to share both output values and captured exceptions.

```csharp
[TestCaseOrderer("Xchain.ChainOrderer", "Xchain")]
public class ChainTest(TestChainFixture chain) : IClassFixture<TestChainFixture>
{
    [ChainFact(Link = 1, Name = "Setup")]
    public void Test1() => chain.Output["Sleep"] = 1500;

    [ChainFact(Link = 2, Name = "Sleep using shared value")]
    public void Test2()
    {
        var sleep = (int)chain.Output["Sleep"];
        Thread.Sleep(sleep);
    }
}
```


## Skipping on Previous Failures

```csharp
[ChainFact(Link = 3, Name = "Failing Root")]
public void Root() => throw new TimeoutException();

[ChainFact(Link = 4, Name = "Skip if Exception")]
public void Dependent() => chain.LinkUnless<Exception>((output) =>
{
    // This test will be skipped
});
```


## Traits with Custom Attributes

You can define custom metadata for filtering and categorization.

```csharp
[TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]
[AttributeUsage(AttributeTargets.Method)]
public class ChainTagAttribute(string? owner = null, string? category = null, string? color = null)
    : Attribute, ITraitAttribute
{
    public string? Owner { get; set; } = owner;
    public string? Category { get; set; } = category;
    public string? Color { get; set; } = color;
}
```

Usage:

```csharp
[ChainFact(Link = 1, Name = "Custom Tagged")]
[ChainTag(Owner = "Dev", Category = "Regression", Color = "Red")]
public void TaggedTest() => ...
```


## Test Output Preview

```
Xchain.Tests.ChainTest: #1 | Sleep 1 second        ✅ Passed
Xchain.Tests.ChainTest: #2 | Sleep 2 seconds       ✅ Passed
Xchain.Tests.ChainTest: #3 | Throw Exception       ❌ Failed
Xchain.Tests.ChainTest: #4 | Fails again           ⚠️ Skipped due to prior failure
Xchain.Tests.ChainTest: #5 | Yet another fail      ⚠️ Skipped due to prior failure
```


## Summary

| Feature              | Description |
|----------------------|-------------|
| `ChainFact(Link)`    | Defines order and enables chaining |
| `Name`               | Sets test display name (with `#Link`) |
| `Pad`                | Pads link number (e.g., `#01`) |
| `LinkUnless<T>`      | Skips if specific exception occurred |
| `Output[...]`        | Share data between tests |
| `ChainTagAttribute`  | Adds test traits dynamically |


## Use Custom Attribute to set Flow for test collection

The `ChainFactAttribute` supports a `Flow` property, allowing test cases to be grouped under a common flow name. While `ChainFactAttribute` is part of the library, you can define your own custom attributes by inheriting from it.

One example is `FlowFactAttribute`, which sets a default flow name for all test cases in a test class. This avoids repeating the `Flow = "..."` assignment in each test case.

#### Purpose

- Demonstrates how to inherit from `ChainFactAttribute`.
- Centralizes the `Flow` definition in a single place.
- Reduces redundancy in test annotations.

#### Example

```csharp
// User-defined attribute
class FlowFactAttribute : ChainFactAttribute { public FlowFactAttribute() => Flow = "MyFlow"; }
```

Usage in tests:

```csharp
[FlowFact(Link = 10, Name = "Sleep 1 second")]
public async Task Test1() => await chain.LinkAsync(...);

[FlowFact(Link = 20, Name = "Sleep 2 seconds")]
public async Task Test2() => await chain.LinkUnlessAsync<Exception>(...);
```

This is equivalent to using:

```csharp
[ChainFact(Flow = "MyFlow", Link = 10, Name = "Sleep 1 second")]
```

but without repeating the `Flow` parameter in every test case.





## Collection-Level Ordering

Xchain supports chaining and data sharing **across test collections** using `ChainLinkAttribute` and `CollectionChainFixture`.

### ChainLink Attribute & ChainLinker

Use `[ChainLink(int)]` to assign a sequence to your test collections. Collections are ordered at runtime using the `ChainLinker` test collection orderer.

###  Collection Fixture: `CollectionChainFixture`

Unlike `TestChainFixture`, which is scoped per class, `CollectionChainFixture` allows shared output **across multiple test classes and collections**. This enables interdependent suites to access a common state.

> ⚠️ Output keys must be **unique across collections**.

---

### Example Setup

#### 1. Define Collections with `ChainLink`

```csharp
[CollectionDefinition("ChainTest")]
[ChainLink(1)]
public class ChainCollection : ICollectionFixture<CollectionChainFixture> { }

[CollectionDefinition("LinkedTest")]
[ChainLink(2)]
public class LinkedCollection : ICollectionFixture<CollectionChainFixture> { }

[CollectionDefinition("LastTest")]
[ChainLink(3)]
public class LastCollection : ICollectionFixture<CollectionChainFixture> { }
```

#### 2. Configure Assembly for Ordered Execution

```csharp
[assembly: TestCollectionOrderer("Xchain.ChainLinker", "Xchain")]
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 0)]
```

> You must **disable parallelization** to ensure predictable ordering across collections.
> You can use parallelization if required. 

#### 3. Use `CollectionChainFixture` in Your Tests

```csharp
[Collection("ChainTest")]
[TestCaseOrderer("Xchain.ChainOrderer", "Xchain")]
public class ChainTest(CollectionChainFixture chain)
{
    [ChainFact(Link = 1, Name = "Sleep 1 second")]
    public async Task Test1() => await chain.LinkAsync(async (output, token) =>
    {
        const int sleep = 1000;
        output["Sleep"] = sleep * 2;
        await Task.Delay(sleep, token);
    });

    [ChainFact(Link = 2, Name = "Sleep 2 seconds")]
    public async Task Test2() => await chain.LinkUnlessAsync<NotImplementedException>(async (output, token) =>
    {
        var sleep = output.Get<int>("Sleep");
        await Task.Delay(sleep, token);
    });

    [ChainFact(Link = 3, Name = "Fail")]
    public void Test3() => chain.Link(() =>
    {
        throw new NotImplementedException();
    });
}
```

```csharp
[Collection("LinkedTest")]
public class LinkedTest(CollectionChainFixture chain)
{
    [Fact]
    public void TestA() => chain.Link(output =>
    {
        Thread.Sleep(1000);
    });

    [Fact]
    public void TestB() => chain.Link(output =>
    {
        Thread.Sleep(1000);
        throw new NotImplementedException();
    });
}
```

```csharp
[Collection("LastTest")]
public class LastTest(CollectionChainFixture chain)
{
    [Fact]
    public void FinalTest() => chain.Link(output =>
    {
        var sleep = output.Get("Sleep");
        Console.WriteLine($"Final sleep value: {sleep}");
        Thread.Sleep(1000);
    });
}
```



### Summary of New Features

| Feature                        | Description                                       |
|-------------------------------|---------------------------------------------------|
| `ChainLinkAttribute`          | Assigns execution order to test collections      |
| `ChainLinker`                 | Orders collections based on `ChainLink`          |
| `CollectionChainFixture`      | Shares output across multiple test classes       |
| `TestCollectionOrderer`       | Assembly-level setup for cross-collection order  |
| `DisableTestParallelization`  | Ensures sequential collection execution          |



## Synchronizing Test Collections with ChainLinkFixture and ChainAwaiterFixture

Xchain supports runtime coordination between test collections using `ChainLinkFixture` and `ChainAwaiterFixture`. This allows a test collection to **start only after another collection has completed**, even when collections are allowed to run in parallel globally.

### Purpose

- Coordinate execution between specific test collections without disabling global parallelization.
- Prevent race conditions or conflicts when collections depend on shared state or resources.
- Maintain logical execution order **beyond static collection ordering**.


### Usage Overview

> - `ChainAwaiter` is an internal component.  
> -  Users interact only with:
> - `ChainLinkFixture` to **register** a collection under a name/key.
> - `ChainAwaiterFixture` to **wait for** a registered collection to finish.


### Example: Coordinated Collections

```csharp
// Registers the collection as "WaitForMe"
public class LongRunningCollectionFixture() 
    : ChainLinkFixture("WaitForMe");

// Waits until "WaitForMe" is completed before starting
public class WaitForLongRunningCollectionFixture() 
    : ChainAwaiterFixture("WaitForMe");
```

Define your collections:

```csharp
[CollectionDefinition("First")]
public class FirstCollection : ICollectionFixture<CollectionChainFixture> { };

// Waits for "WaitForMe" before executing
[CollectionDefinition("Second")]
public class SecondCollection 
    : ICollectionFixture<WaitForLongRunningCollectionFixture>, 
      ICollectionFixture<CollectionChainFixture> { };

// Registers itself as "WaitForMe"
[CollectionDefinition("Third")]
public class LinkedCollection 
    : ICollectionFixture<LongRunningCollectionFixture>, 
      ICollectionFixture<CollectionChainFixture> { }

// Also waits for "WaitForMe"
[CollectionDefinition("Four")]
public class LastCollection 
    : ICollectionFixture<WaitForLongRunningCollectionFixture>, 
      ICollectionFixture<CollectionChainFixture> { };
```

In this scenario:

- `LinkedCollection` (Third) begins first and **registers itself** as `"WaitForMe"` using `ChainLinkFixture`.
- `SecondCollection` and `LastCollection` use `ChainAwaiterFixture("WaitForMe")` to **delay execution** until the registration is marked as complete.



### Optional Timeout

You can specify a timeout for how long a collection should wait:

```csharp
public class WaitWithTimeoutFixture() 
    : ChainAwaiterFixture("WaitForMe", TimeSpan.FromMinutes(2)) { }
```

---

### Summary

| Component                        | Purpose                                                   |
|----------------------------------|-----------------------------------------------------------|
| `ChainLinkFixture(name)`         | Registers the current collection under a given key        |
| `ChainAwaiterFixture(name)`      | Waits until the named collection has completed            |
| `ChainAwaiter` (internal)        | Manages registration and synchronization internally       |

> This mechanism allows partial parallelization with explicit coordination where needed.



---

- Powered by [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact)
- Created from [JandaBox](https://github.com/Jandini/JandaBox)
- Box icon by [Freepik – Flaticon](https://www.flaticon.com/free-icons/box)

