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

### Simplified: `CollectionChainNextFixture<TAwait, TRegister>`

For a collection that both **depends on** another collection and **is itself a dependency** for something downstream, use the combined fixture:

```csharp
[CollectionDefinition("SecondCollection")]
public class SecondCollectionDefinition :
    ICollectionFixture<CollectionChainNextFixture<ProducerCollection, ConsumerCollection>>,
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

`CollectionChainNextFixture<TAwait, TRegister>` does two things in one:
1. Blocks until `TAwait` (the producer) completes before any tests in this collection start.
2. Registers `TRegister` (this collection) so further downstream collections can wait for it.

The default timeout is 360 seconds. To use a custom timeout, subclass it:

```csharp
internal class MyFixture : CollectionChainNextFixture<ProducerCollection, ConsumerCollection>
{
    public MyFixture() : base(TimeSpan.FromMinutes(5)) { }
}
```

### Explicit: separate Signal and Await fixtures

Use this when a collection only needs to wait (not signal), or when you need `IMessageSink` diagnostics:

```csharp
// Producer — signals itself, does not wait for anything
[CollectionDefinition("FirstCollection")]
public class FirstCollectionDefinition :
    ICollectionFixture<ProducerSignalFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class ProducerSignalFixture : CollectionChainSignalFixture<ProducerCollection>;

[Collection("FirstCollection")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ProducerCollection(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create Resource")]
    public void Step1() =>
        chain.Link(output => output["ResourceId"] = CreateResource());
}


// Consumer — waits for producer, does not signal itself
[CollectionDefinition("SecondCollection")]
public class SecondCollectionDefinition :
    ICollectionFixture<ProducerAwaitFixture>,
    ICollectionFixture<CollectionChainContextFixture>;

internal class ProducerAwaitFixture : CollectionChainAwaitFixture<ProducerCollection>;

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

The key is automatically prefixed with the fully qualified type name (e.g., `MyNamespace.ProducerCollection_ResourceId`), making it unique per collection even across namespaces.

```csharp
// Producer
chain.Link(output => output.ResourceId<ProducerCollection>().Put(Guid.NewGuid()));

// Consumer
chain.LinkWithCollection<ProducerCollection>(
    output.ResourceId<ProducerCollection>(),
    output => Assert.NotEqual(Guid.Empty, output.ResourceId<ProducerCollection>().Get()));
```

---

## Reusable Chain Templates

When the same sequence of steps needs to run against multiple subjects (e.g. two different clients, two different projects), duplicating test classes is fragile and hard to maintain. Xchain solves this with a pattern that combines abstract base test classes, three collection definition helpers, and a generic parameter trick to keep output keys isolated.

### The scenario

Three templates, two parallel workflows:

```
ClientA ──► ProjectA ──► ImportA
ClientB ──► ProjectB ──► ImportB
```

`CreateClientChain`, `CreateProjectChain`, and `ImportDataChain` are each written once. `ClientA`/`ClientB`, `ProjectA`/`ProjectB`, and `ImportA`/`ImportB` are six concrete classes that inherit from those templates. Both workflows run in parallel — they share no dependencies and can race freely.

### Execution timeline

```
────────────────────────────────────────────────── time ──────────────────────────────────►

ClientA  ██████████████▶ signal
ClientB  ██████████▶ signal                         (A and B run in parallel)

                  ProjectA  (awaits ClientA) ██████████████▶ signal
                  ProjectB  (awaits ClientB) ██████████▶ signal

                                       ImportA  (awaits ProjectA) ████████
                                       ImportB  (awaits ProjectB) ████████
```

### Collection definition helpers

Every collection in a chain needs fixtures to wire up the signal/await coordination. Xchain provides three abstract base classes so this reduces to a single line per collection:

| Base class | Position | What it provides |
|---|---|---|
| `CollectionChainStartDefinition<T>` | First — signals only | `CollectionChainSignalFixture<T>` + `CollectionChainContextFixture` |
| `CollectionChainNextDefinition<TAwait, T>` | Middle — awaits predecessor + signals self | `CollectionChainNextFixture<TAwait, T>` + `CollectionChainContextFixture` |
| `CollectionChainEndDefinition<TAwait>` | Last — awaits only, no signal | `CollectionChainAwait<TAwait>` + `CollectionChainContextFixture` |

```csharp
[CollectionDefinition("FlowA_01_Client")]
public class ClientADefinition : CollectionChainStartDefinition<ClientA>;

[CollectionDefinition("FlowA_02_Project")]
public class ProjectADefinition : CollectionChainNextDefinition<ClientA, ProjectA>;

[CollectionDefinition("FlowA_03_Import")]
public class ImportADefinition : CollectionChainEndDefinition<ProjectA>;
```

### The CRTP pattern — output isolation

When `ClientA` and `ClientB` both run the same `CreateClientChain` template, they must not overwrite each other's output keys. The solution is the **CRTP pattern** (Curiously Recurring Template Pattern): the abstract base takes the concrete subclass as a generic parameter, so it can generate a namespaced key without the subclass doing anything extra.

```csharp
// General shape: the base "knows" the subclass type via TSelf
abstract class Base<TSelf> { }
class Derived : Base<Derived> { }   // TSelf = Derived inside Base
```

Applied here:

```csharp
public abstract class CreateClientChain<TSelf>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1)]
    public void CreateClient() =>
        chain.Link(output => output.ClientId<TSelf>().Put(Guid.NewGuid()));
}

public class ClientA(...) : CreateClientChain<ClientA>(...);  // TSelf = ClientA
public class ClientB(...) : CreateClientChain<ClientB>(...);  // TSelf = ClientB
```

`TestOutput<ClientA, Guid>` generates key `"MyTests.ClientA_ClientId"` and `TestOutput<ClientB, Guid>` generates `"MyTests.ClientB_ClientId"`. The keys are structurally distinct — no naming convention required, no runtime check.

### `[TestCaseOrderer]` inheritance

Place `[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]` on the abstract template base, **not** on every concrete class. xUnit inherits attributes declared with `Inherited = true`, and `TestCaseOrdererAttribute` is one of them — all subclasses pick it up automatically.

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]   // ← declared once here
public abstract class CreateClientChain<TSelf>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create client")]
    public void CreateClient() => ...

    [ChainFact(Link = 2, Name = "Verify client")]
    public void VerifyClient() => ...
}

[Collection("ClientA")]
public class ClientA(...) : CreateClientChain<ClientA>(...);  // [TestCaseOrderer] inherited
```

### Full example — one workflow

**Step 1: output key extensions**

```csharp
public static class ChainOutputKeys
{
    public static TestOutput<T, Guid>   ClientId<T>(this TestChainOutput o) => new(o, "ClientId");
    public static TestOutput<T, string> ProjectId<T>(this TestChainOutput o) => new(o, "ProjectId");
    public static TestOutput<T, string> ImportId<T>(this TestChainOutput o) => new(o, "ImportId");
}
```

**Step 2: templates**

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class CreateClientChain<TSelf>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create client")]
    public void CreateClient() =>
        chain.Link(output => output.ClientId<TSelf>().Put(Guid.NewGuid()));

    [ChainFact(Link = 2, Name = "Verify client")]
    public void VerifyClient() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEqual(Guid.Empty, output.ClientId<TSelf>().Get()));
}

// TClient is the upstream collection type — used to read its output key
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class CreateProjectChain<TSelf, TClient>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create project")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<TClient>(), output =>
        {
            var clientId = output.ClientId<TClient>().Get();
            output.ProjectId<TSelf>().Put($"project-for-{clientId}");
        });

    [ChainFact(Link = 2, Name = "Verify project")]
    public void VerifyProject() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEmpty(output.ProjectId<TSelf>().Get()));
}

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class ImportDataChain<TSelf, TProject>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Import data")]
    public void ImportData() =>
        chain.LinkWithCollection(chain.Output.ProjectId<TProject>(), output =>
        {
            var projectId = output.ProjectId<TProject>().Get();
            output.ImportId<TSelf>().Put($"import-for-{projectId}");
        });

    [ChainFact(Link = 2, Name = "Verify import")]
    public void VerifyImport() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEmpty(output.ImportId<TSelf>().Get()));
}
```

**Step 3: workflow A instances**

```csharp
[CollectionDefinition("FlowA_01_Client")]
public class ClientADefinition : CollectionChainStartDefinition<ClientA>;

[Collection("FlowA_01_Client")]                 // [TestCaseOrderer] inherited from base
public class ClientA(CollectionChainContextFixture chain) : CreateClientChain<ClientA>(chain);


[CollectionDefinition("FlowA_02_Project")]
public class ProjectADefinition : CollectionChainNextDefinition<ClientA, ProjectA>;

[Collection("FlowA_02_Project")]
public class ProjectA(CollectionChainContextFixture chain) : CreateProjectChain<ProjectA, ClientA>(chain);


[CollectionDefinition("FlowA_03_Import")]
public class ImportADefinition : CollectionChainEndDefinition<ProjectA>;

[Collection("FlowA_03_Import")]
public class ImportA(CollectionChainContextFixture chain) : ImportDataChain<ImportA, ProjectA>(chain);
```

**Step 4: workflow B — same templates, different instances**

```csharp
[CollectionDefinition("FlowB_01_Client")]
public class ClientBDefinition : CollectionChainStartDefinition<ClientB>;

[Collection("FlowB_01_Client")]
public class ClientB(CollectionChainContextFixture chain) : CreateClientChain<ClientB>(chain);


[CollectionDefinition("FlowB_02_Project")]
public class ProjectBDefinition : CollectionChainNextDefinition<ClientB, ProjectB>;

[Collection("FlowB_02_Project")]
public class ProjectB(CollectionChainContextFixture chain) : CreateProjectChain<ProjectB, ClientB>(chain);


[CollectionDefinition("FlowB_03_Import")]
public class ImportBDefinition : CollectionChainEndDefinition<ProjectB>;

[Collection("FlowB_03_Import")]
public class ImportB(CollectionChainContextFixture chain) : ImportDataChain<ImportB, ProjectB>(chain);
```

> **Constraints**
>
> - **Type name must be unique**: the awaiter coordination key and `TestOutput` key both use `typeof(T).FullName`. If two classes share the same full name (namespace + class), they will collide. This is a compile-time error in C#, so it can't happen accidentally.
> - **`TAwait` and upstream type param must agree**: `ProjectADefinition : CollectionChainNextDefinition<ClientA, ProjectA>` awaits `ClientA`, and `CreateProjectChain<ProjectA, ClientA>` reads `ClientA`'s output. These two `ClientA` references must match — there is no compiler check across the definition class and test class.
> - **`CollectionChainAwait<T>` vs `CollectionChainAwaitFixture<T>`**: use `CollectionChainAwait<T>` in `ICollectionFixture` declarations (single constructor). Use `CollectionChainAwaitFixture<T>` only when subclassing to provide a custom timeout.
> - **`TestOutput.Key` uses `FullName`**: the generated key is `"My.Namespace.ClassName_suffix"`. Raw string access like `output["ClientA_suffix"]` breaks — use `TestOutput<T>` accessors which read the key dynamically.

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

These types are marked `[Obsolete]`. They required `[assembly: CollectionBehavior(DisableTestParallelization = true)]` which blocks all parallel test execution. Use `CollectionChainNextFixture<TAwait, TRegister>`, `CollectionChainSignalFixture<T>`, and `CollectionChainAwaitFixture<T>` instead — they coordinate collections without disabling parallelism.

---

- Powered by [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact)
- Created from [JandaBox](https://github.com/Jandini/JandaBox)
- Icon by [Freepik – Flaticon](https://www.flaticon.com/free-icon/link_1325130?term=chain&page=5&position=11&origin=search&related_id=1325130)
