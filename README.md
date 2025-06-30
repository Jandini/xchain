# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

**Xchain** extends xUnit with structured test chaining, shared context between steps, and smart skipping based on previous failures. It's ideal for integration tests, API flows, or any scenario where test steps depend on each other and need to run in a defined order.

## Features

- Chain test steps using `[ChainFact]` or `[ChainTheory]` with readable display names.
- Share structured state via `TestChainOutput`.
- Automatically skip downstream tests when failures occur.
- Chain multiple test **collections**, synchronizing execution with full isolation.
- Filter test runs using custom traits with strongly-typed metadata.
- Compatible with async workflows and supports rich diagnostics.

---

## Why Xchain?

Traditional xUnit tests are isolated by design — great for unit tests, but limiting for integration or scenario tests where later steps depend on earlier ones.

**Xchain solves this** by enabling:
- Logical chaining within a class (`Link`-ordered `[ChainFact]`)
- Shared, strongly-typed test output values
- Explicit collection orchestration
- Exception tracking and conditional skipping
- Filtering with trait-based metadata

---

## How It Works

Xchain uses xUnit’s extensibility to provide:
- Custom `[ChainFact]` / `[ChainTheory]` attributes that inject ordering, naming, and flow grouping.
- A fixture-based pattern (`TestChainContextFixture`) for output sharing and exception flow.
- Fluent APIs like `.Link`, `.LinkUnless`, `.LinkWithCollection`, etc. to capture and control flow.
- Collection coordination using dedicated `Setup` and `Await` fixtures.
- Strong typing for dictionary-style output sharing.

---

## Quick Start

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class SimpleTestChain(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Prepare Data")]
    public void Step1() =>
        chain.Link(output => output["Token"] = "abc123");

    [ChainFact(Link = 2, Name = "Use Data")]
    public void Step2() =>
        chain.Link(output =>
        {
            var token = output.Get<string>("Token");
            Assert.Equal("abc123", token);
        });
}
```


Each `[ChainFact]` defines a link in the chain. You can pass values between steps using `output`, and if a previous step fails, later steps can be automatically skipped.


## Key Concepts

Xchain builds upon xUnit's extensibility to introduce a minimal and powerful abstraction for test chaining. Here are the key features and how they work:

### `ChainFact` and `ChainTheory`

These are enhanced versions of `[Fact]` and `[Theory]` that introduce:

- **Link-based ordering**: Test execution is driven by an integer `Link`, which determines order within the test class.
- **Flow grouping**: An optional `Flow` label allows grouping and visualizing test steps in Test Explorer.
- **Named steps**: The `Name` property contributes to human-readable, sortable test display names.

```csharp
[ChainFact(Link = 10, Name = "Login", Flow = "User Flow")]
public void LoginTest() => ...
```

The display name will appear as:

```
#10 | User Flow | Login
```

This improves traceability in test logs and IDE explorers.

### Linking Logic (`Link`, `LinkUnless`, `LinkAsync`, etc.)

Xchain provides fluent methods to structure test execution:

- `Link`: Run test logic and track exceptions.
- `LinkUnless<TException>`: Skip test if a specific exception already occurred.
- `LinkAsync`: Asynchronous variant with timeout support.
- `SkipIf<TException>`: Explicitly skip a test if a condition exists in error history.

Each method records failures into a structured error stack (`TestChainErrors`), and subsequent steps can respond accordingly.

### Shared Context (`TestChainOutput`)

A shared, thread-safe dictionary allows test steps to share values:

```csharp
chain.Link(output => output["UserId"] = 123);
...
chain.Link(output =>
{
    var userId = output.Get<int>("UserId");
    ...
});
```

You can also build reusable strongly typed accessors using `TestOutput<TCollection, TOutput>` to ensure key uniqueness across collections.

### Chaining Collections

Xchain enables one collection to wait for another to finish using:

- `CollectionChainLinkSetupFixture<T>` — registers a collection as a chain step.
- `CollectionChainLinkAwaitFixture<T>` — blocks until the registered collection completes.

This is essential for cross-collection orchestration:

```csharp
public class SetupFixture : CollectionChainLinkSetupFixture<MyCollection>;
public class AwaitFixture : CollectionChainLinkAwaitFixture<MyCollection>;
```

You can then fluently access shared output using:

```csharp
chain.LinkWithCollection<PreviousCollection>(..., ...)
```

Note: Collection timeouts default to **6 minutes**. This can be configured in the `CollectionChainLinkAwaitFixture`.

### Metadata via Traits

Xchain includes a flexible `TraitDiscoverer` that allows users to define their own strongly-typed attributes for tagging and organizing tests. These attributes can be applied at both the class and method levels, enabling structured metadata that can be used for filtering, grouping, and diagnostics.

Below are two examples of custom attributes that a user of Xchain might define.

#### Example: `MetadataAttribute` (Simple Category Tag)

This attribute adds a single category to a test class or method. It's useful for basic test grouping.

```csharp
using Xunit.Sdk;

namespace Xchain.Tests;

[TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MetadataAttribute(string category) : Attribute, ITraitAttribute
{
    public string Category { get; } = category;
}
```

**Usage:**

```csharp
[Metadata("Xchain Collection")]
[Collection("SecondCollection")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ConsumerCollection(CollectionChainContextFixture chain)
    : IClassFixture<CollectionChainContextFixture>
{
    // Chained tests
}
```

```csharp
[Metadata("SmokeTest")]
public async Task SmokeTest() => await chain.LinkAsync(...);
```

#### Example: `ChainTagAttribute` (Rich Metadata)

This attribute demonstrates how to attach multiple pieces of metadata to a test, such as owner, category, and color. It can be used for diagnostics, ownership tracking, and enhanced reporting.

```csharp
using Xunit.Sdk;

namespace Xchain.Tests;

[TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ChainTagAttribute(string? owner = null, string? category = null, string? color = null)
    : Attribute, ITraitAttribute
{
    public string? Owner { get; set; } = owner;
    public string? Category { get; set; } = category;
    public string? Color { get; set; } = color;
}
```

**Usage on a test method:**

```csharp
[FlowFact(Link = 10, Name = "Sleep 1 second")]
[ChainTag(Owner = "Kethoneinuo", Category = "Important", Color = "Black")]
public async Task Test3() =>
    await chain.LinkAsync(async (output, cancellationToken) =>
    {
        const int sleep = 1000;
        output["Sleep"] = sleep * 2;
        await Task.Delay(sleep, cancellationToken);
    }, TimeSpan.FromMilliseconds(100));
```

**Usage on a test class:**

```csharp
[ChainTag(Owner = "QA Team", Category = "Regression", Color = "Green")]
public class RegressionSuite
{
    // Test methods here
}
```

This can be used to group collections and run them by trait.

### Optional Collection Orderer

You may also define collection-wide execution order using:

```csharp
[CollectionDefinition("MyCollection")]
[CollectionChainOrder(1)]
public class OrderedCollection { }
```

However, this requires global test parallelism to be disabled, so it is generally less recommended than explicit `Await`/`Setup` chaining.

---

Each of these features builds on xUnit principles while maintaining full compatibility. You can opt-in incrementally — use as little or as much as your test architecture requires.



## Creating Strongly Typed Output Keys

When working with shared outputs across tests or collections, it's helpful to avoid using raw string keys. Xchain supports strongly typed access via `TestOutput<TCollection, TOutput>`, which encapsulates the key naming and value casting.

### Step 1: Define a Typed Extension

Create an extension method to encapsulate a reusable key. Here's an example for a shared `Guid`:

```csharp
public static class TestChainOutputExtensions
{
    public static TestOutput<T, Guid> SharedId<T>(this TestChainOutput output) =>
        new(output, "SharedId");
}
```

The `T` type ensures key uniqueness by prefixing it with the type name (e.g., `SetupTests_SharedId`), making it safe across multiple collections.

### Step 2: Store a Value in the Output

In the producing test or collection, use `Put` to store a value:

```csharp
[Collection("SetupCollection")]
public class SetupTests(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Generate Id")]
    public void GenerateId() =>
        chain.Link(output => output.SharedId<SetupTests>().Put(Guid.NewGuid()));
}
```

### Step 3: Retrieve the Value from Another Collection

In the consuming test, use `Get` to retrieve the value:

```csharp
[Collection("ConsumerCollection")]
public class ConsumerTests(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Read Shared Id")]
    public void ReadId() =>
        chain.Link(output =>
        {
            var id = output.SharedId<SetupTests>().Get();
            Assert.NotEqual(Guid.Empty, id);
        });
}
```

### Benefits

- **Type-safe** access to output data.
- **Fluent syntax** for both producers and consumers.
- **Unique scoping** of keys per test or collection type.
- **Simplified refactoring**, avoiding scattered string literals.

This pattern helps enforce clarity and consistency when passing values between chained tests.



---

- Powered by [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact)
- Created from [JandaBox](https://github.com/Jandini/JandaBox)
- Icon by [Freepik – Flaticon](https://www.flaticon.com/free-icon/link_1325130?term=chain&page=5&position=11&origin=search&related_id=1325130)

