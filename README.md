# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

**Xchain** extends xUnit with ordered, dependent test steps. Steps share state through a typed dictionary, and if a step fails, downstream steps can automatically skip instead of failing with a misleading error.

---

## Setup

Install from NuGet:

```
dotnet add package xchain
```

Every test class that uses chaining needs two things: the `TestChainContextFixture` injected as a constructor parameter, and the `TestChainOrderer` declared on the class.

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class MyChain(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    // steps go here
}
```

---

## Chaining Steps

Use `[ChainFact]` with a `Link` number to define execution order. Call `chain.Link(output => ...)` to run a step and share data.

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class LoginFlow(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Authenticate")]
    public void Step1() =>
        chain.Link(output => output["Token"] = AuthService.Login("user", "pass"));

    [ChainFact(Link = 2, Name = "Fetch Profile")]
    public void Step2() =>
        chain.Link(output =>
        {
            var token = output.Get<string>("Token");
            var profile = ApiClient.GetProfile(token);
            Assert.NotNull(profile);
        });
}
```

`output` is a `ConcurrentDictionary<string, object>` shared across all steps in the class. Any exception thrown inside `Link(...)` is captured into `chain.Errors` and re-thrown, so xUnit still reports the failure.

---

## Skipping on Failure

This is the core feature. When a step fails, later steps that depend on it should **skip**, not fail with a secondary error. Use `LinkUnless<TException>` (sync) or `LinkUnlessAsync<TException>` (async) to skip if a specific exception type already exists in the error stack.

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ApiFlow(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Connect")]
    public async Task Step1_Connect() =>
        await chain.LinkAsync(async (output, token) =>
        {
            output["Client"] = await ApiClient.ConnectAsync(token);
        });

    [ChainFact(Link = 2, Name = "Submit Order")]
    public async Task Step2_Submit() =>
        await chain.LinkUnlessAsync<HttpRequestException>(async (output, token) =>
        {
            var client = output.Get<ApiClient>("Client");
            output["OrderId"] = await client.SubmitOrderAsync(token);
        });

    [ChainFact(Link = 3, Name = "Verify Order")]
    public async Task Step3_Verify() =>
        await chain.LinkUnlessAsync<HttpRequestException>(async (output, token) =>
        {
            var orderId = output.Get<Guid>("OrderId");
            var status = await ApiClient.CheckStatusAsync(orderId, token);
            Assert.Equal("Confirmed", status);
        });
}
```

If `Step1_Connect` throws `HttpRequestException`, steps 2 and 3 are **skipped** rather than failing. The test output distinguishes between a real failure (❌) and a downstream skip (⚠️).

`LinkUnlessAsync<TException>` also accepts an optional timeout:

```csharp
await chain.LinkUnlessAsync<MyException>(async (output, token) =>
{
    // ...
}, TimeSpan.FromSeconds(30));
```

If the timeout elapses, a `TimeoutException` is pushed to the error stack and the test fails — downstream steps that guard on `TimeoutException` will then skip.

**Skip variants:**

| Method | When to use |
|---|---|
| `Link(...)` | Sync step, no skipping |
| `LinkAsync(...)` | Async step, no skipping |
| `LinkUnless<TException>(...)` | Sync step, skip if `TException` already in errors |
| `LinkUnlessAsync<TException>(...)` | Async step, skip if `TException` already in errors |
| `SkipIf<TException>(...)` | Explicit skip check without executing any logic |

---

## Cross-Collection Chains

When your scenario spans multiple xUnit test collections, use `CollectionChainContextFixture` (instead of `TestChainContextFixture`) so that `Output` and `Errors` are shared as static state across all collections.

### Simplified: `CollectionChainFixture<TAwait, TRegister>`

For a collection that both **depends on** another collection and **is itself a dependency** for something downstream, use the combined fixture:

```csharp
[CollectionDefinition("SecondCollection")]
public class SecondCollectionDefinition :
    ICollectionFixture<CollectionChainFixture<ProducerCollection, ConsumerCollection>>,
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("SecondCollection")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ConsumerCollection(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Use Resource")]
    public void Step1() =>
        chain.LinkWithCollection<ProducerCollection>("ResourceId", output =>
        {
            var id = output.Get<string>("ResourceId");
            Assert.NotNull(id);
        });

    // Skips if ProducerCollection threw any exception
    [ChainFact(Link = 2, Name = "Dependent step")]
    public void Step2() =>
        chain.LinkUnless<Exception>(output => { /* ... */ });
}
```

`CollectionChainFixture<TAwait, TRegister>` does two things in one:
1. Blocks until `TAwait` (the producer) completes before any tests in this collection start.
2. Registers `TRegister` (this collection) so further downstream collections can wait for it.

The default timeout is 360 seconds. To use a custom timeout, subclass it:

```csharp
internal class MyFixture : CollectionChainFixture<ProducerCollection, ConsumerCollection>
{
    public MyFixture() : base(TimeSpan.FromMinutes(5)) { }
}
```

### Explicit: separate Setup and Await fixtures

Use this when a collection only needs to wait (not register), or when you need `IMessageSink` diagnostics:

```csharp
// Producer — registers itself, does not wait for anything
[CollectionDefinition("FirstCollection")]
public class FirstCollectionDefinition :
    ICollectionFixture<ProducerSetupFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class ProducerSetupFixture : CollectionChainLinkSetupFixture<ProducerCollection>;

[Collection("FirstCollection")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ProducerCollection(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create Resource")]
    public void Step1() =>
        chain.Link(output => output["ResourceId"] = CreateResource());
}


// Consumer — waits for producer, does not register itself
[CollectionDefinition("SecondCollection")]
public class SecondCollectionDefinition :
    ICollectionFixture<ProducerAwaitFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class ProducerAwaitFixture : CollectionChainLinkAwaitFixture<ProducerCollection>;

[Collection("SecondCollection")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ConsumerCollection(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Use Resource")]
    public void Step1() =>
        chain.LinkWithCollection<ProducerCollection>("ResourceId", output =>
        {
            var id = output.Get<string>("ResourceId");
            Assert.NotNull(id);
        });
}
```

`LinkWithCollection<TCollection>` skips the step if the expected key was not produced — no assertion failure, just a clean skip. The default await timeout is **6 minutes**.

### Static shared state

`CollectionChainContextFixture` uses static fields for both `Output` and `Errors`, so they are shared across every collection that uses this fixture:

- **Output** — use globally unique keys, or use `TestOutput<TCollection, T>` wrappers to auto-namespace them.
- **Errors** — any exception from any collection is visible to all other collections. This means `LinkUnless<TException>` and `SkipIf<TException>` in a consumer will skip correctly if the producer encountered that exception type.

---

## Strongly Typed Output Keys

For cross-collection sharing, wrap keys in a typed accessor to avoid string collisions and get compile-time safety.

```csharp
// Shared extension — accessible from any collection
public static class OutputKeys
{
    public static TestOutput<T, Guid> ResourceId<T>(this TestChainOutput output) =>
        new(output, "ResourceId");
}
```

The key is automatically prefixed with the type name (e.g., `ProducerCollection_ResourceId`), making it unique per collection.

```csharp
// Producer
chain.Link(output => output.ResourceId<ProducerCollection>().Put(Guid.NewGuid()));

// Consumer
chain.LinkWithCollection<ProducerCollection>(
    output.ResourceId<ProducerCollection>(),
    output => Assert.NotEqual(Guid.Empty, output.ResourceId<ProducerCollection>().Get()));
```

---

## Display Names and Traits

`[ChainFact]` and `[ChainTheory]` format their display name as:

```
#<Link> | <Flow> | <Name>
```

```csharp
[ChainFact(Link = 10, Name = "Submit", Flow = "Order Flow")]
```

→ `#10 | Order Flow | Submit`

To set `Flow` and `Pad` once per class rather than repeating them on every method, create a private inner attribute:

```csharp
// Flow groups the steps under a label; Pad=2 zero-pads the Link number so
// steps sort correctly when there are 10+ in the list (#01, #02 … #10).
class OrderFlowFact : ChainFactAttribute
{
    public OrderFlowFact() { Flow = "Order Flow"; Pad = 2; }
}

[OrderFlowFact(Link = 1,  Name = "Authenticate")]   // → #01 | Order Flow | Authenticate
[OrderFlowFact(Link = 2,  Name = "Submit")]          // → #02 | Order Flow | Submit
[OrderFlowFact(Link = 10, Name = "Confirm")]         // → #10 | Order Flow | Confirm
```

Without `Pad`, `Link = 10` sorts before `Link = 2` alphabetically in some test explorers (`#10` < `#2`). Setting `Pad = 2` produces `#02` and `#10`, which sort correctly.

To tag tests for filtering, define a custom attribute with `[TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]`. Xchain reflects over its public properties and exposes each as an xUnit trait:

```csharp
[TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TagAttribute(string category) : Attribute, ITraitAttribute
{
    public string Category { get; } = category;
}
```

```bash
dotnet test --filter "Category=Smoke"
```

---

## Breaking Changes

### Errors are now shared across collections

`CollectionChainContextFixture.Errors` is now a **static** stack (matching the existing behavior of `Output`). In earlier versions, each collection instance had its own independent error stack; errors from one collection were not visible to others.

**Impact:** `chain.LinkUnless<TException>()` and `chain.SkipIf<TException>()` in a consumer collection will now skip if that exception type was thrown in any prior collection. This is the intended behavior for chained integration flows where a failure in collection A should propagate skips to collection B.

If you have collections that share `CollectionChainContextFixture` but are **not** logically related and should not share errors, use separate fixture types instead.

### `CollectionChainOrderer` and `CollectionChainOrderAttribute` are deprecated

These types are marked `[Obsolete]`. They required `[assembly: CollectionBehavior(DisableTestParallelization = true)]` which blocks all parallel test execution. Use `CollectionChainFixture<TAwait, TRegister>`, `CollectionChainLinkSetupFixture<T>`, and `CollectionChainLinkAwaitFixture<T>` instead — they coordinate collections without disabling parallelism.

---

- Powered by [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact)
- Created from [JandaBox](https://github.com/Jandini/JandaBox)
- Icon by [Freepik – Flaticon](https://www.flaticon.com/free-icon/link_1325130?term=chain&page=5&position=11&origin=search&related_id=1325130)
