# XChain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

**XChain** extends xUnit with structured test chaining, shared context between steps, and smart skipping based on previous failures. It's ideal for integration tests, API flows, or any scenario where test steps depend on each other and need to run in a defined order.

## Features

- Chain test steps using `[ChainFact]` or `[ChainTheory]` with readable display names.
- Share structured state via `TestChainOutput`.
- Automatically skip downstream tests when failures occur.
- Chain multiple test **collections**, synchronizing execution with full isolation.
- Filter test runs using custom traits with strongly-typed metadata.
- Compatible with async workflows and supports rich diagnostics.

---

## Why XChain?

Traditional xUnit tests are isolated by design — great for unit tests, but limiting for integration or scenario tests where later steps depend on earlier ones.

**XChain solves this** by enabling:
- Logical chaining within a class (`Link`-ordered `[ChainFact]`)
- Shared, strongly-typed test output values
- Explicit collection orchestration
- Exception tracking and conditional skipping
- Filtering with trait-based metadata

---

## How It Works

XChain uses xUnit’s extensibility to provide:
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

XChain builds upon xUnit's extensibility to introduce a minimal and powerful abstraction for test chaining. Here are the key features and how they work:

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

XChain provides fluent methods to structure test execution:

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

XChain enables one collection to wait for another to finish using:

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

### Global Metadata via Traits

XChain includes a `TraitDiscoverer` that turns strongly-typed attributes into metadata for filtering and organizing tests.

```csharp
[Metadata("SmokeTests")]
public class MyTestSuite { }
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

When working with shared outputs across tests or collections, it's helpful to avoid using raw string keys. XChain supports strongly typed access via `TestOutput<TCollection, TOutput>`, which encapsulates the key naming and value casting.

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

