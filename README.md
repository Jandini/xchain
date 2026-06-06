# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

**Xchain** extends xUnit with ordered, dependent test steps. Steps share state through a typed dictionary, and if a step fails, downstream steps automatically skip instead of failing with a misleading error.

```
dotnet add package xchain
```

---

## Single Chains

A **single chain** is a sequence of ordered steps inside one test class. Each step can pass data to the next, and a failure in an early step causes later steps to skip — not fail with a secondary error.

### Setup

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class LoginFlow(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    // steps go here
}
```

Two things are required:
- `IClassFixture<TestChainContextFixture>` — creates the shared fixture for this class
- `[TestCaseOrderer]` — tells xUnit to sort test methods by their `Link` number

### Writing Steps

Use `[ChainFact(Link = N)]` to define execution order. Call `chain.Link(output => ...)` to run a step and share data between steps:

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class LoginFlow(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Authenticate")]
    public void Step1() =>
        chain.Link(output =>
        {
            var token = AuthService.Login("user", "pass");
            output["Token"] = token;
        });

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

`output` is a `ConcurrentDictionary<string, object>` shared across all steps in the class.

### Skipping on Failure

Use `LinkUnless<TException>` to skip a step when a specific exception type has already been captured by a prior step:

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class ApiFlow(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    [ChainFact(Link = 1, Name = "Connect")]
    public void Step1() =>
        chain.Link(output =>
        {
            output["Client"] = ApiClient.Connect(); // throws HttpRequestException on failure
        });

    [ChainFact(Link = 2, Name = "Submit Order")]
    public void Step2() =>
        chain.LinkUnless<HttpRequestException>(output => // skipped if Step1 threw
        {
            output["OrderId"] = output.Get<ApiClient>("Client").SubmitOrder();
        });

    [ChainFact(Link = 3, Name = "Verify Order")]
    public void Step3() =>
        chain.LinkUnless<HttpRequestException>(output => // skipped if Step1 or Step2 threw
        {
            Assert.Equal("Confirmed", ApiClient.CheckStatus(output.Get<Guid>("OrderId")));
        });
}
```

If `Step1` throws `HttpRequestException`, steps 2 and 3 are **skipped** (⚠️) not failed (❌). Only the root cause is reported.

### Async Steps and Timeouts

All `Link` methods have async variants. Pass an optional `TimeSpan` to enforce a timeout:

```csharp
[ChainFact(Link = 1, Name = "Connect")]
public async Task Step1() =>
    await chain.LinkAsync(async (output, ct) =>
    {
        output["Client"] = await ApiClient.ConnectAsync(ct);
    });

[ChainFact(Link = 2, Name = "Long operation")]
public async Task Step2() =>
    await chain.LinkUnlessAsync<HttpRequestException>(async (output, ct) =>
    {
        await DoWorkAsync(ct);
    }, TimeSpan.FromSeconds(30));
```

If the timeout elapses, `TimeoutException` is pushed to the error stack. Downstream steps guarding on it will skip.

### Step Reference

| Method | Sync/Async | Skips when exception already in errors |
|---|---|---|
| `Link(...)` | sync | no |
| `LinkAsync(...)` | async | no |
| `LinkUnless<TException>(...)` | sync | yes |
| `LinkUnlessAsync<TException>(...)` | async | yes |
| `SkipIf<TException>(...)` | — | explicit skip, no logic runs |

---

## Display Names and Custom Fact Attributes

`[ChainFact]` and `[ChainTheory]` format their display name as:

```
#<Link> | <Flow> | <Name>
```

```csharp
[ChainFact(Link = 10, Name = "Submit", Flow = "Order Flow")]
// → "#10 | Order Flow | Submit"
```

### Why `Pad` Matters

Without padding, `Link = 10` sorts before `Link = 2` alphabetically in some test explorers (`"#10"` < `"#2"`). Set `Pad = 2` to zero-pad the link number:

```
Pad = 0 (default):  #1, #2, #10  → sorts as: #1, #10, #2  ✗
Pad = 2:            #01, #02, #10 → sorts as: #01, #02, #10 ✓
```

### Creating a Custom Fact Attribute

Writing `Flow = "..."` and `Pad = 2` on every test method is tedious and error-prone. Subclass `ChainFactAttribute` to set them once and use the subclass on every method.

For **non-generic** classes, a nested private subclass works well:

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class OrderFlow(TestChainContextFixture chain) : IClassFixture<TestChainContextFixture>
{
    class Step : ChainFactAttribute
    {
        public Step() { Flow = "Order Flow"; Pad = 2; }
    }

    [Step(Link = 1,  Name = "Authenticate")]   // → #01 | Order Flow | Authenticate
    public void Step1() => chain.Link(_ => { });

    [Step(Link = 2,  Name = "Submit")]          // → #02 | Order Flow | Submit
    public void Step2() => chain.Link(_ => { });

    [Step(Link = 10, Name = "Confirm")]         // → #10 | Order Flow | Confirm
    public void Step10() => chain.Link(_ => { });
}
```

> **Generic classes only:** C# does not allow a type nested inside a generic class to be used as an attribute (CS0416 — the nested type's full name includes the open type parameter). For abstract template classes, define the attribute outside the generic class — see the Templates section below.

The same technique works for `ChainTheoryAttribute`.

---

## Workflow Chains

A **workflow chain** coordinates multiple xUnit test collections that execute in a controlled order and share state. Each collection waits for its upstream to complete before starting.

```
Collection A  ──►  Collection B  ──►  Collection C
```

Use `CollectionChainContextFixture` instead of `TestChainContextFixture` — its `Output` and `Errors` are static, visible across all collections.

### Producer / Consumer

A producer collection **signals** when it finishes. A consumer collection **awaits** that signal before starting.

```csharp
// ── Producer ──────────────────────────────────────────────────────────────
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

// ── Consumer ──────────────────────────────────────────────────────────────
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

`LinkWithCollection<TCollection>(key, action)` validates that `key` exists in the shared output before running `action`. If the key is absent because the upstream failed or was skipped, this step skips cleanly.

### Middle Collections (`CollectionChainNextFixture`)

A collection that both waits for an upstream and signals a downstream uses the combined fixture:

```csharp
[CollectionDefinition("MiddleCollection")]
public class MiddleCollectionDefinition :
    ICollectionFixture<CollectionChainNextFixture<ProducerCollection, MiddleCollection>>,
    ICollectionFixture<CollectionChainContextFixture>;
```

To customize the timeout (default: 6 minutes):

```csharp
internal class MyFixture : CollectionChainNextFixture<ProducerCollection, MiddleCollection>
{
    public MyFixture() : base(TimeSpan.FromMinutes(2)) { }
}
```

### Definition Base Classes

| Base class | Position | Provides |
|---|---|---|
| `CollectionChainStartDefinition<T>` | First — signals only | Signal + Context |
| `CollectionChainNextDefinition<TAwait, T>` | Middle — awaits + signals | NextFixture + Context |
| `CollectionChainEndDefinition<TAwait>` | Last — awaits only | Await + Context |

These reduce each definition to a single line:

```csharp
[CollectionDefinition("ClientA")]
public class Step_01_ClientDefinition : CollectionChainStartDefinition<Step_01_Client>;

[CollectionDefinition("ProjectA")]
public class Step_02_ProjectDefinition : CollectionChainNextDefinition<Step_01_Client, Step_02_Project>;

[CollectionDefinition("ImportA")]
public class Step_03_ImportDefinition : CollectionChainNextDefinition<Step_02_Project, Step_03_Import>;
```

### Fan-In: Depending on Multiple Upstream Collections

When a collection depends on more than one upstream, declare the await fixtures explicitly:

```csharp
[CollectionDefinition("FlowC_Project")]
public class Step_01_ProjectDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA.Step_01_Client>>,   // waits for FlowA client
    ICollectionFixture<CollectionChainAwait<FlowB.Step_03_Import>>,   // waits for FlowB import
    ICollectionFixture<CollectionChainSignalFixture<Step_01_Project>>,// signals self
    ICollectionFixture<CollectionChainContextFixture>;
```

> Use `CollectionChainAwait<T>` in `ICollectionFixture<>` declarations (single constructor). Use `CollectionChainAwaitFixture<T>` only when subclassing to provide a custom timeout.

---

## Typed Output Keys

String keys (`output["Token"]`) are fine for single chains. For workflow chains where multiple collections share a dictionary, typed keys prevent collisions and catch renames at compile time.

### Defining Typed Keys

Define extension methods on `TestChainOutput` using `nameof` so the key stays in sync with the method name:

```csharp
public static class OutputKeys
{
    public static TestOutput<T, Guid>   ClientId<T>(this TestChainOutput o)  => new(o, nameof(ClientId));
    public static TestOutput<T, string> ProjectId<T>(this TestChainOutput o) => new(o, nameof(ProjectId));
    public static TestOutput<T, string> ImportId<T>(this TestChainOutput o)  => new(o, nameof(ImportId));
}
```

The key for `output.ClientId<FlowA.Step_01_Client>()` is generated as `"MyProject.FlowA.Step_01_Client_ClientId"` — the full type name plus the method name — unique by construction.

### Usage

```csharp
// Store a value
chain.Link(output => output.ClientId<TSelf>().Put(Guid.NewGuid()));

// Retrieve a value
var id = output.ClientId<TSelf>().Get();

// Guard: step skips if key is absent (upstream failed or was skipped)
chain.LinkWithCollection(chain.Output.ClientId<TClient>(), output =>
{
    var clientId = output.ClientId<TClient>().Get();
    // ...
});
```

`TestOutput<TCollection, TOutput>` also provides:
- `TryGet(out TOutput value)` — safe retrieval without throwing
- `ContainsKey()` — check presence without retrieving

---

## Traits

Tag tests for `dotnet test --filter` with a custom `ITraitAttribute`. Xchain reflects over its public properties and exposes each as an xUnit trait:

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

## Reusable Chain Templates

When the same sequence of steps needs to run against multiple subjects — two clients, two environments, two API versions — duplicate test classes are fragile. Use abstract base classes so the logic is written once and inherited by each concrete flow.

### The CRTP Pattern — Output Key Isolation

Pass the concrete subclass as a generic parameter (`TSelf`). The base class uses this to namespace output keys per subclass, so two flows running the same template never overwrite each other's data:

Because `CreateClientChain<TSelf>` is generic, the attribute must be defined outside the class. An `internal class` in the same namespace (or the same file) works cleanly:

```csharp
// Defined outside the generic class — can be in the same file or a separate Domain file
internal class CreateClientStepAttribute : ChainFactAttribute
{
    public CreateClientStepAttribute() { Flow = "Create Client"; Pad = 2; }
}

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class CreateClientChain<TSelf>(CollectionChainContextFixture chain)
{
    [CreateClientStep(Link = 1, Name = "Create client")]
    public void CreateClient() =>
        chain.Link(output => output.ClientId<TSelf>().Put(Guid.NewGuid()));

    [CreateClientStep(Link = 2, Name = "Verify client")]
    public void VerifyClient() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEqual(Guid.Empty, output.ClientId<TSelf>().Get()));
}
```

Concrete step classes inherit the tests, the orderer, and the custom fact attribute:

```csharp
// FlowA/Step_01_Client.cs
[Collection("ClientA")]
public class Step_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<Step_01_Client>(chain);   // TSelf = FlowA.Step_01_Client

// FlowB/Step_01_Client.cs
[Collection("ClientB")]
public class Step_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<Step_01_Client>(chain);   // TSelf = FlowB.Step_01_Client
```

Their output keys are derived from the full type name, so they never collide:

```
FlowA.Step_01_Client → "MyProject.FlowA.Step_01_Client_ClientId"
FlowB.Step_01_Client → "MyProject.FlowB.Step_01_Client_ClientId"
```

### `[TestCaseOrderer]` Inheritance

Place `[TestCaseOrderer]` on the **abstract base class** — xUnit inherits it to all subclasses automatically. Concrete classes need nothing extra.

A useful correctness check: declare `VerifyClient` (Link = 2) before `CreateClient` (Link = 1) in source order. If the orderer is not inherited, `VerifyClient` runs first, calls `.Get()` on a key that doesn't exist yet, and throws — immediately flagging the misconfiguration.

### Full Example: Three Templates, Two Flows, One Fan-In

```
FlowA:  Step_01_Client ──► Step_02_Project ──► Step_03_Import ──┐
              │                                                   │
FlowC:        └─────────────── Step_01_Project ◄─────────────────┘
                                    ▲
FlowB:  Step_01_Client ──► Step_02_Project ──► Step_03_Import ──┘
```

**Abstract templates (written once):**

```csharp
internal class CreateProjectStepAttribute : ChainFactAttribute
{
    public CreateProjectStepAttribute() { Flow = "Create Project"; Pad = 2; }
}

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class CreateProjectChain<TSelf, TClient>(CollectionChainContextFixture chain)
{
    [CreateProjectStep(Link = 1, Name = "Create project")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<TClient>(), output =>
        {
            var clientId = output.ClientId<TClient>().Get();
            output.ProjectId<TSelf>().Put($"project-for-{clientId}");
        });

    [CreateProjectStep(Link = 2, Name = "Verify project")]
    public void VerifyProject() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEmpty(output.ProjectId<TSelf>().Get()));
}

internal class ImportDataStepAttribute : ChainFactAttribute
{
    public ImportDataStepAttribute() { Flow = "Import Data"; Pad = 2; }
}

[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public abstract class ImportDataChain<TSelf, TProject>(CollectionChainContextFixture chain)
{
    [ImportDataStep(Link = 1, Name = "Import data")]
    public void ImportData() =>
        chain.LinkWithCollection(chain.Output.ProjectId<TProject>(), output =>
        {
            var projectId = output.ProjectId<TProject>().Get();
            output.ImportId<TSelf>().Put($"import-for-{projectId}");
        });

    [ImportDataStep(Link = 2, Name = "Verify import")]
    public void VerifyImport() =>
        chain.LinkUnless<Exception>(output =>
            Assert.NotEmpty(output.ImportId<TSelf>().Get()));
}
```

**Concrete steps (one file each, FlowA shown — FlowB is identical, different namespace):**

```csharp
// FlowA/Step_01_Client.cs
[CollectionDefinition("ClientA")]
public class Step_01_ClientDefinition : CollectionChainStartDefinition<Step_01_Client>;

[Collection("ClientA")]
public class Step_01_Client(CollectionChainContextFixture chain)
    : CreateClientChain<Step_01_Client>(chain);

// FlowA/Step_02_Project.cs
[CollectionDefinition("ProjectA")]
public class Step_02_ProjectDefinition : CollectionChainNextDefinition<Step_01_Client, Step_02_Project>;

[Collection("ProjectA")]
public class Step_02_Project(CollectionChainContextFixture chain)
    : CreateProjectChain<Step_02_Project, Step_01_Client>(chain);

// FlowA/Step_03_Import.cs
[CollectionDefinition("ImportA")]
public class Step_03_ImportDefinition : CollectionChainNextDefinition<Step_02_Project, Step_03_Import>;

[Collection("ImportA")]
public class Step_03_Import(CollectionChainContextFixture chain)
    : ImportDataChain<Step_03_Import, Step_02_Project>(chain);
```

**Fan-in — FlowC depends on outputs from both FlowA and FlowB:**

```csharp
namespace MyProject.FlowC;

[CollectionDefinition("FlowC_Project")]
public class Step_01_ProjectDefinition :
    ICollectionFixture<CollectionChainAwait<FlowA.Step_01_Client>>,
    ICollectionFixture<CollectionChainAwait<FlowB.Step_03_Import>>,
    ICollectionFixture<CollectionChainSignalFixture<Step_01_Project>>,
    ICollectionFixture<CollectionChainContextFixture>;

[Collection("FlowC_Project")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class Step_01_Project(CollectionChainContextFixture chain)
{
    class Fact : ChainFactAttribute
    {
        public Fact() { Flow = "FlowC Project"; Pad = 2; }
    }

    [Fact(Link = 1, Name = "Create cross-flow project")]
    public void CreateProject() =>
        chain.LinkWithCollection(chain.Output.ClientId<FlowA.Step_01_Client>(), output =>
        {
            var clientId = output.ClientId<FlowA.Step_01_Client>().Get();
            var importId = output.ImportId<FlowB.Step_03_Import>().Get();
            output.ProjectId<Step_01_Project>().Put($"client={clientId}|import={importId}");
        });
}
```

### Recommended Project Layout

Keep template flows in their own project so they run independently, without sharing error state with the rest of the suite:

```
src/
  MyProject.Tests/
  MyProject.Tests.Templates/
    Domain/
      CreateClientChain.cs
      CreateProjectChain.cs
      ImportDataChain.cs
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
```

Prefix step class names with a number so Test Explorer sorts them in execution order (`Step_01_`, `Step_02_`, `Step_03_`). Without the prefix, alphabetical order disagrees with execution order.

---

## Advanced: Source Generator

Xchain ships a Roslyn source generator that eliminates the `[CollectionDefinition]` and fixture boilerplate from workflow chains. Instead of writing definition classes and fixture subclasses by hand, you declare the chain topology once and mark step classes `partial` — the generator wires everything at build time.

### Declaring a Workflow

```csharp
public class MyWorkflow : WorkflowChain
{
    protected override void Configure(IWorkflowBuilder b) => b
        .Start<Step_01_Create>()
        .Then<Step_02_Use>()
        .End<Step_03_Verify>();
}
```

| Method | Effect |
|---|---|
| `Start<T>()` | First collection — signals itself on completion |
| `Then<T>()` | Middle collection — awaits predecessor, signals itself |
| `End<T>()` | Last collection — awaits predecessor |
| `After<T1>()` / `After<T1, T2>()` | Cross-workflow upstream dependencies |

### Step Classes

Mark each step class `partial` and drop `[Collection]` / `[CollectionDefinition]` — the generator adds them:

```csharp
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public partial class Step_01_Create(CollectionChainContextFixture chain)
{
    [ChainFact(Link = 1, Name = "Create resource")]
    public void Create() =>
        chain.Link(output => output["ResourceId"] = CreateResource());
}
```

If you forget `partial`, the compiler raises `CS0260` pointing directly to the problem class.

### What the Generator Produces

For each step it emits a `[Collection]` partial and a `[CollectionDefinition]` class using the type's `FullName` as the collection name:

```csharp
// auto-generated
[Collection("MyNamespace.Step_01_Create")]
public partial class Step_01_Create { }

[CollectionDefinition("MyNamespace.Step_01_Create")]
public class Step_01_CreateDefinition : CollectionChainStartDefinition<Step_01_Create> { }
```

### Output Schema Generator

Instead of writing `TestOutput<T>` extension methods by hand, annotate an interface with `[ChainOutputSchema]`. The generator emits one typed extension method per property:

```csharp
[ChainOutputSchema]
public interface IMyOutputs
{
    Guid   ClientId  { get; }
    string ProjectId { get; }
    string ImportId  { get; }
}
// Generator emits: output.ClientId<T>(), output.ProjectId<T>(), output.ImportId<T>()
```

Renaming a property is a compile error at every call site — no magic strings, no runtime key drift.

### Fan-In with the Generator

```csharp
public class WorkflowC : WorkflowChain
{
    protected override void Configure(IWorkflowBuilder b) => b
        .After<FlowA.Step_01_Client, FlowB.Step_03_Import>()
        .Start<FlowC.Step_01_Project>();
}
```

---

## Breaking Changes

### Errors are now shared across collections

`CollectionChainContextFixture.Errors` is now a **static** stack (matching the existing behavior of `Output`). Previously each collection instance had its own error stack. Now `LinkUnless<TException>` in a consumer will skip if that exception was thrown in any prior collection.

If you have collections that share `CollectionChainContextFixture` but are **not** logically related, use separate fixture types so they don't share error state.

### `CollectionChainOrderer` and `CollectionChainOrderAttribute` are deprecated

These types required `[assembly: CollectionBehavior(DisableTestParallelization = true)]`, which blocks all parallel execution. Use `CollectionChainNextFixture<TAwait, TRegister>`, `CollectionChainSignalFixture<T>`, and `CollectionChainAwaitFixture<T>` instead.

---

- Powered by [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact)
- Created from [JandaBox](https://github.com/Jandini/JandaBox)
- Icon by [Freepik – Flaticon](https://www.flaticon.com/free-icon/link_1325130?term=chain&page=5&position=11&origin=search&related_id=1325130)
