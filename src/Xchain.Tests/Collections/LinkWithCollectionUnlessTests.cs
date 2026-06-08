using Xchain;

namespace Xchain.Tests.Collections;

// Unique exception type used only by these tests — keeps the error-stack check isolated
// from the deliberate failures in other test classes (InvalidOperationException, TimeoutException, etc.)
internal class UnlessTestException(string message) : Exception(message);

// ── Producer collection ──────────────────────────────────────────────────────────────

[CollectionDefinition("UnlessProducer")]
public class UnlessProducerDefinition : CollectionChainStartDefinition<UnlessProducerCollection>;

[Collection("UnlessProducer")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class UnlessProducerCollection(CollectionChainContextFixture chain)
{
    // Writes a key that consumers can read.
    [ChainFact(Link = 1, Name = "Write success key")]
    public void WriteSuccessKey() =>
        chain.Link(output => output["UnlessSuccessKey"] = "produced-value");

    // Simulates a prior failure by pushing directly to the shared error stack.
    // This test PASSES (no throw) but leaves UnlessTestException in Errors
    // so the consumer can verify the skip-on-exception behavior.
    [ChainFact(Link = 2, Name = "Push prior UnlessTestException to error stack")]
    public void PushPriorException()
    {
        var inner = new UnlessTestException("simulated upstream failure");
        chain.Errors.Push(new TestChainException(inner, chain.Errors, nameof(PushPriorException), "", -1));
    }
}

// ── Consumer collection ──────────────────────────────────────────────────────────────

[CollectionDefinition("UnlessConsumer")]
public class UnlessConsumerDefinition : CollectionChainEndDefinition<UnlessProducerCollection>;

[Collection("UnlessConsumer")]
[TestCaseOrderer("Xchain.TestChainOrderer", "Xchain")]
public class LinkWithCollectionUnlessTests(CollectionChainContextFixture chain)
{
    // UnlessTestException IS in Errors AND key EXISTS → skips on exception (exception takes priority).
    [ChainFact(Link = 1, Name = "Skip: prior exception takes priority over key existence")]
    public void SkipsWhenExceptionExists_EvenIfKeyPresent() =>
        chain.LinkWithCollectionUnless<UnlessTestException, UnlessProducerCollection>(
            "UnlessSuccessKey",
            _ => Assert.Fail("Should have been skipped because UnlessTestException is in Errors"));

    // ArgumentException is NOT in Errors, and key EXISTS → executes normally.
    [ChainFact(Link = 2, Name = "Execute: key present and no matching exception")]
    public void ExecutesWhenKeyPresentAndNoMatchingException() =>
        chain.LinkWithCollectionUnless<ArgumentException, UnlessProducerCollection>(
            "UnlessSuccessKey",
            output => Assert.Equal("produced-value", output.Get<string>("UnlessSuccessKey")));

    // ArgumentException is NOT in Errors, and key IS MISSING → skips on missing key.
    [ChainFact(Link = 3, Name = "Skip: key missing and no matching exception")]
    public void SkipsWhenKeyMissing() =>
        chain.LinkWithCollectionUnless<ArgumentException, UnlessProducerCollection>(
            "UnlessMissingKey",
            _ => Assert.Fail("Should have been skipped because key is missing"));
}
