# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from the `src/` directory.

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~SimpleChainTest"

# Run tests by trait (e.g. custom MetadataAttribute category)
dotnet test --filter "Category=SmokeTest"

# Pack the NuGet package (Release)
dotnet pack -c Release -o nuget
```

Versioning is managed by `GitVersion.MsBuild` (see `GitVersion.yml`). NuGet pushes are triggered in CI only when the commit message contains `prerelease`.

## Architecture

**Xchain** is a single-assembly xUnit extension (`src/Xchain/`, targets `netstandard2.1`) with a companion test project (`src/Xchain.Tests/`, targets `net8.0`).

### Core abstractions

| Type | Role |
|---|---|
| `ChainFactAttribute` / `ChainTheoryAttribute` | Drop-in replacements for `[Fact]`/`[Theory]`. Add `Link` (execution order), `Flow` (grouping label), and `Name` (display name). Display name format: `#<Link> \| <Flow> \| <Name>`. |
| `TestChainOrderer` | `ITestCaseOrderer` that sorts tests by their `Link` value. Must be declared via `[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]` on each test class. |
| `TestChainOutput` | `ConcurrentDictionary<string, object>` — shared state passed into every `Link*` lambda. |
| `TestChainErrors` | Stack of `TestChainException` — accumulated failure history for the current fixture scope. |
| `TestChainContextFixture` | Per-class fixture (`IClassFixture<T>`). Holds `Output` and `Errors`. |
| `CollectionChainContextFixture` | Cross-collection fixture. `Output` is a `static` field shared across all collections; use globally unique keys or `TestOutput<TCollection, T>` wrappers. |
| `CollectionChainSignalFixture<T>` | Registers collection T with the internal awaiter on construction; unregisters (signals done) on disposal. |
| `CollectionChainAwaitFixture<T>` | Blocks fixture construction until T's collection signals completion. Four constructors (timeout / IMessageSink variants). |
| `CollectionChainAwait<T>` | Single-constructor wrapper around `CollectionChainAwaitFixture<T>` for direct use in `ICollectionFixture<>` declarations. |
| `CollectionChainNextFixture<TAwait, T>` | Combined: awaits TAwait then signals T. Replaces the separate signal + await two-fixture pattern. |
| `CollectionChainStartDefinition<T>` | Abstract `[CollectionDefinition]` base for the first collection in a chain. Inherits Signal + Context fixtures. |
| `CollectionChainNextDefinition<TAwait, T>` | Abstract `[CollectionDefinition]` base for middle collections. Inherits NextFixture + Context fixtures. |
| `CollectionChainEndDefinition<TAwait>` | Abstract `[CollectionDefinition]` base for the last collection in a chain. Inherits Await + Context fixtures. |

### Extension methods (the actual API)

All public API lives in two static extension classes:

- **`TestChainContextFixtureExtensions`** — used with `TestChainContextFixture` or `CollectionChainContextFixture`:
  - `Link(Action<TestChainOutput>)` / `Link<TResult>(Func<...>)` — execute a step, push any exception to `Errors`.
  - `LinkUnless<TException>(...)` — skip if `TException` is already in `Errors`.
  - `LinkAsync(...)` / `LinkUnlessAsync<TException>(...)` — async variants; accept optional `TimeSpan timeOut`.
  - `SkipIf<TException>(...)` — explicit skip without executing any logic.

- **`CollectionChainContextFixtureExtensions`** — used only with `CollectionChainContextFixture`:
  - `LinkWithCollection<TCollection>(outputKey, ...)` — validates key exists in shared output before executing; skips if missing.
  - `LinkWithCollectionAsync<TCollection, TOutput>(...)` — async variant.

### Cross-collection orchestration

Use `CollectionChainSignalFixture<T>` and `CollectionChainAwaitFixture<T>` as assembly fixtures to synchronize two collections. The await fixture blocks its collection's fixture construction until the target collection signals completion (default 360-second timeout).

```csharp
// In the depending collection's definition:
public class MyAwait : CollectionChainAwaitFixture<ProducerCollection>;
```

### Reusable chain templates

For running the same chain steps against multiple subjects (e.g. two clients), use the CRTP pattern: declare `abstract class CreateClientChain<TSelf>` and inherit as `class ClientA : CreateClientChain<ClientA>`. `TSelf` is used in `TestOutput<TSelf, T>` to auto-namespace output keys per instance. Put `[TestCaseOrderer]` on the abstract base — xUnit inherits it. See README "Reusable Chain Templates" and `src/Xchain.Tests/Templates/` for a full example.

### Strongly typed output keys

`TestOutput<TCollection, TOutput>` namespaces a string key by prefixing it with the **fully qualified** type name (`FullName ?? Name`), preventing collisions in the global `CollectionChainContextFixture.Output`. Define these as extension methods on `TestChainOutput` and use `.Put(value)` / `.Get()` for type-safe access.

### Trait metadata

`TraitDiscoverer` is a generic `ITraitDiscoverer` that reflects over any `ITraitAttribute` and emits all its public properties as xUnit traits. Consumers define their own attribute classes (e.g. `MetadataAttribute`, `ChainTagAttribute`) decorated with `[TraitDiscoverer("Xchain.TraitDiscoverer", "Xchain")]`.
