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

Three templates (`CreateClientChain`, `CreateProjectChain`, `ImportDataChain`), each written once and instantiated into two independent parallel flows:

```
FlowA:  Step_01_Client ──► Step_02_Project ──► Step_03_Import
FlowB:  Step_01_Client ──► Step_02_Project ──► Step_03_Import
```

Both flows run in parallel — they share no dependencies and can race freely. A third flow (`FlowC`) demonstrates fan-in: it has no client of its own and instead awaits results from both upstream flows before running.

```
FlowA:  Step_01_Client ────────────────────────────────────────► Step_02_Project ──► Step_03_Import
                     │                                                                              
FlowC:               └─────────────────────────────────────────────────────────► Step_01_Project
                                                                                  ▲
FlowB:  Step_01_Client ──► Step_02_Project ──► Step_03_Import ───────────────────┘
```

### Execution timeline

```
────────────────────────────────────────────────────── time ──────────────────────────────────►

FlowA:  Step_01_Client  ██████████████▶ signal
FlowB:  Step_01_Client  ██████████▶ signal          (flows start in parallel)

        FlowA: Step_02_Project  (awaits FlowA client) ██████████████▶ signal
        FlowB: Step_02_Project  (awaits FlowB client) ██████████▶ signal

                FlowA: Step_03_Import  (awaits FlowA project) ████████▶ signal
                FlowB: Step_03_Import  (awaits FlowB project) ████████▶ signal

                        FlowC: Step_01_Project  (awaits FlowA client + FlowB import) ████████
```

### Collection definition helpers

Every collection in a chain needs fixtures to wire up the signal/await coordination. Xchain provides three abstract base classes so this reduces to a single line per collection:

| Base class | Position | What it provides |
|---|---|---|
| `CollectionChainStartDefinition<T>` | First — signals only | `CollectionChainSignalFixture<T>` + `CollectionChainContextFixture` |
| `CollectionChainNextDefinition<TAwait, T>` | Middle — awaits predecessor + signals self | `CollectionChainNextFixture<TAwait, T>` + `CollectionChainContextFixture` |
| `CollectionChainEndDefinition<TAwait>` | Last — awaits only, no signal | `CollectionChainAwait<TAwait>` + `CollectionChainContextFixture` |

```csharp
[CollectionDefinition("ClientA")]
public class Step_01_ClientDefinition : CollectionChainStartDefinition<Step_01_Client>;

[CollectionDefinition("ProjectA")]
public class Step_02_ProjectDefinition : CollectionChainNextDefinition<Step_01_Client, Step_02_Project>;

[CollectionDefinition("ImportA")]
public class Step_03_ImportDefinition : CollectionChainNextDefinition<Step_02_Project, Step_03_Import>;
```

### The CRTP pattern — output isolation

When FlowA and FlowB both run the same `CreateClientChain` template, they must not overwrite each other's output keys. The solution is the **CRTP pattern** (Curiously Recurring Template Pattern): the abstract base takes the concrete subclass as a generic parameter so it can namespace output keys automatically — without the subclass doing anything extra.

```csharp
// General shape: the base "knows" the subclass type via TSelf
abstract class Base<TSelf> { }
class Derived : Base<Derived> { }   // TSelf = Derived inside Base
```

Applied here, each flow's `Step_01_Client` passes itself as `TSelf`. Because the two classes live in different namespaces (`FlowA` vs `FlowB`), their `FullName` values differ and so do their output keys:

```
FlowA.Step_01_Client → key "MyProject.FlowA.Step_01_Client_ClientId"
FlowB.Step_01_Client → key "MyProject.FlowB.Step_01_Client_ClientId"
```

No naming convention required — the isolation is structural.

### `[TestCaseOrderer]` inheritance

Place `[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]` on the abstract template base, **not** on every concrete class. xUnit inherits attributes declared with `Inherited = true`, and `TestCaseOrdererAttribute` is one of them — all subclasses pick it up automatically.

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]   // ← declared once on the base
public abstract class CreateClientChain<TSelf>(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 2, Name = "Verify client")]   // declared first in source...
    public void VerifyClient() => ...               // ...but runs second (Link=2)

    [ChainFact(Link = 1, Name = "Create client")]   // declared second in source...
    public void CreateClient() => ...               // ...but runs first (Link=1)
}

[Collection("ClientA")]
public class Step_01_Client(...) : CreateClientChain<Step_01_Client>(...);  // orderer inherited
```

Deliberately declaring `VerifyClient` (Link=2) before `CreateClient` (Link=1) in source order acts as a built-in mechanism check: if the orderer is not inherited, `VerifyClient` runs first, calls `.Get()` on a key that doesn't exist yet, and throws.

### Full example — output key extensions and templates

**Output key extensions** (defined once, shared across all flows):

```csharp
public static class ChainOutputKeys
{
    public static TestOutput<T, Guid>   ClientId<T>(this TestChainOutput o) => new(o, "ClientId");
    public static TestOutput<T, string> ProjectId<T>(this TestChainOutput o) => new(o, "ProjectId");
    public static TestOutput<T, string> ImportId<T>(this TestChainOutput o) => new(o, "ImportId");
}
```

**Abstract templates** (defined once, inherited by every flow):

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

**Concrete flow instances** (one file per step, grouped by namespace):

```csharp
// namespace MyProject.FlowA
[CollectionDefinition("ClientA")]
public class Step_01_ClientDefinition : CollectionChainStartDefinition<Step_01_Client>;

[Collection("ClientA")]  // [TestCaseOrderer] inherited
public class Step_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<Step_01_Client>(chain);

// ---

[CollectionDefinition("ProjectA")]
public class Step_02_ProjectDefinition : CollectionChainNextDefinition<Step_01_Client, Step_02_Project>;

[Collection("ProjectA")]
public class Step_02_Project(CollectionChainContextFixture chain)
    : CreateProjectChain<Step_02_Project, Step_01_Client>(chain);

// ---

[CollectionDefinition("ImportA")]
public class Step_03_ImportDefinition : CollectionChainNextDefinition<Step_02_Project, Step_03_Import>;

[Collection("ImportA")]
public class Step_03_Import(CollectionChainContextFixture chain)
    : ImportDataChain<Step_03_Import, Step_02_Project>(chain);
```

FlowB is identical in structure — same templates, different namespace (`MyProject.FlowB`), different `[CollectionDefinition]` names (`"ClientB"`, `"ProjectB"`, `"ImportB"`).

### Fan-in: depending on multiple upstream flows

A collection can await more than one upstream by declaring fixtures inline. When there are two or more upstream dependencies the definition base classes (`CollectionChainStartDefinition`, `CollectionChainNextDefinition`) cannot be used — each accepts only one upstream type.

`FlowC.Step_01_Project` fans in from both FlowA's client and FlowB's completed import:

```csharp
using FlowA = MyProject.FlowA;
using FlowB = MyProject.FlowB;

namespace MyProject.FlowC;

[CollectionDefinition("FlowC_Project")]
public class Step_01_ProjectDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA.Step_01_Client>>,    // waits for FlowA client
    ICollectionFixture<CollectionChainAwait<FlowB.Step_03_Import>>,    // waits for FlowB import
    ICollectionFixture<CollectionChainSignalFixture<Step_01_Project>>, // signals self for downstream
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("FlowC_Project")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class Step_01_Project(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create cross-flow project")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<FlowA.Step_01_Client>(), output =>
        {
            var clientId = output.ClientId<FlowA.Step_01_Client>().Get();
            var importId = output.ImportId<FlowB.Step_03_Import>().Get();
            output.ProjectId<Step_01_Project>().Put($"cross-flow|client={clientId}|import={importId}");
        });
}
```

Namespace aliases (`using FlowA = ...`) resolve the ambiguity when multiple namespaces contain classes with the same name.

### Organizing for the test runner

#### Step naming

Visual Studio Test Explorer sorts classes alphabetically within a namespace. Prefix class names with a step number to match execution order:

```
Step_01_Client    ← appears first  (C < I < P without prefix)
Step_02_Project   ← appears second
Step_03_Import    ← appears third
```

Without the prefix, `Import` would sort before `Project` alphabetically — the opposite of execution order.

#### Namespace grouping

Put each flow in its own sub-namespace. VS Test Explorer groups by namespace, so all steps of a flow appear together and the flows are visually separated:

```
Namespace: MyProject.FlowA
  Class: Step_01_Client
  Class: Step_02_Project
  Class: Step_03_Import
Namespace: MyProject.FlowB
  Class: Step_01_Client
  Class: Step_02_Project
  Class: Step_03_Import
Namespace: MyProject.FlowC
  Class: Step_01_Project
```

The namespace also provides the uniqueness that makes CRTP work: `FlowA.Step_01_Client` and `FlowB.Step_01_Client` are different types with different `FullName` values, so their output keys never collide even though the class name is identical.

#### Separate test project

Template-based flows are best kept in their own test project so they can be run independently — without pulling in the rest of the test suite's shared static error state.

```
src/
  MyProject.Tests/              ← existing tests
  MyProject.Tests.Templates/    ← flows live here
    FlowA/
      Step_01_Client.cs
      Step_02_Project.cs
      Step_03_Import.cs
    FlowB/
      Step_01_Client.cs
      Step_02_Project.cs
      Step_03_Import.cs
    FlowC/
      Step_01_Project.cs
    CreateClientChain.cs
    CreateProjectChain.cs
    ImportDataChain.cs
    ChainOutputKeys.cs
```

Run just the template flows:

```bash
dotnet test MyProject.Tests.Templates
```

> **Constraints**
>
> - **Type `FullName` must be unique**: the awaiter coordination key and `TestOutput` key both use `typeof(T).FullName`. The namespace is part of `FullName`, so two classes with the same name in different namespaces are always distinct. A true collision (same namespace + same class name) is a compile-time error in C#.
> - **`TAwait` and upstream type param must agree**: in `CollectionChainNextDefinition<Step_01_Client, Step_02_Project>`, the `Step_01_Client` awaited by the definition must be the same type as the `TClient` in `CreateProjectChain<Step_02_Project, Step_01_Client>`. There is no compiler check across these two declarations — by convention they must match.
> - **`CollectionChainAwait<T>` vs `CollectionChainAwaitFixture<T>`**: use `CollectionChainAwait<T>` in `ICollectionFixture` declarations (single constructor). Use `CollectionChainAwaitFixture<T>` only when subclassing to provide a custom timeout.
> - **`TestOutput.Key` uses `FullName`**: the generated key is `"My.Namespace.ClassName_suffix"`. Raw string access like `output["Step_01_Client_ClientId"]` breaks — use `TestOutput<T>` accessors which read the key dynamically.

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
