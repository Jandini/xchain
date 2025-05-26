# Xchain

[![Build](https://github.com/Jandini/Xchain/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/Xchain/actions/workflows/nuget.yml)

Xchain extends xUnit with a fluent mechanism to **chain tests**, **pass data**, and **skip dependent tests** if previous ones fail — ideal for integration or system tests with interdependencies.

---

## ✨ Features

- **Chained execution**: Tests can conditionally run based on previous outcomes.
- **Shared output state**: Tests exchange data via `TestChainFixture`.
- **Skips on failure**: Later tests are skipped if earlier ones fail.
- **Custom ordering**: Tests are run in a defined sequence using `Link`.
- **Named tests**: Set display name with `Name`, auto-prepended with `# Link`.
- **Zero-padded sorting**: Use `Pad` to ensure consistent numeric display alignment.
- **Custom metadata**: Add test traits via simple attribute classes.

---

## 🧪 Example: Chained Execution with Display Names

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

---

## Fluent Chaining Methods

- `Link` — executes and captures exceptions  
- `LinkUnless<TException>` — skips test if exception `TException` was previously thrown  
- `LinkAsync` / `LinkUnlessAsync<TException>` — async equivalents  

---

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

---

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

---

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

---

## Test Output Preview

```
Xchain.Tests.ChainTest.#1 | Sleep 1 second        ✅ Passed
Xchain.Tests.ChainTest.#2 | Sleep 2 seconds       ✅ Passed
Xchain.Tests.ChainTest.#3 | Throw Exception       ❌ Failed
Xchain.Tests.ChainTest.#4 | Fails again           ⚠️ Skipped due to prior failure
Xchain.Tests.ChainTest.#5 | Yet another fail      ⚠️ Skipped due to prior failure
```

---

## Summary

| Feature              | Description |
|----------------------|-------------|
| `ChainFact(Link)`    | Defines order and enables chaining |
| `Name`               | Sets test display name (with `#Link`) |
| `Pad`                | Pads link number (e.g., `#01`) |
| `LinkUnless<T>`      | Skips if specific exception occurred |
| `Output[...]`        | Share data between tests |
| `ChainTagAttribute`  | Adds test traits dynamically |

---

- Powered by [Xunit.SkippableFact](https://github.com/AArnott/Xunit.SkippableFact)
- Created from [JandaBox](https://github.com/Jandini/JandaBox)
- Box icon by [Freepik – Flaticon](https://www.flaticon.com/free-icons/box)
