using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class LegacyConfigurationSaveTests
{
    [Fact]
    public async Task InitialCancellationDoesNotPrepareOrAcquireTheGate()
    {
        using var gate = new SemaphoreSlim(1, 1);
        using var stop = new CancellationTokenSource();
        stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(gate, "unused", new Document(Guid.Empty, ""),
                _ => throw new InvalidOperationException("Preparation must not run"),
                _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
                new JsonSerializerOptions(), stop.Token));
        Assert.Equal(1, gate.CurrentCount);
    }

    [Fact]
    public async Task CancellationWhileWaitingDoesNotReleaseAnUnacquiredGate()
    {
        using var gate = new SemaphoreSlim(0, 1);
        using var stop = new CancellationTokenSource();
        Task? pending = null;
        try
        {
            pending = ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(gate, "unused", new Document(Guid.Empty, ""),
                _ => throw new InvalidOperationException("Preparation must not run"),
                _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
                new JsonSerializerOptions(), stop.Token);
            Assert.False(pending.IsCompleted);
            stop.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
            Assert.Equal(0, gate.CurrentCount);
        }
        finally
        {
            stop.Cancel();
            if (pending is not null)
                try { await pending; }
                catch (OperationCanceledException) { }
        }
    }

    [Fact]
    public async Task PreparationFailureReleasesTheAcquiredGate()
    {
        using var gate = new SemaphoreSlim(1, 1);
        var failure = new InvalidOperationException("Preparation failure");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(gate, "unused", new Document(Guid.Empty, ""),
                _ =>
                {
                    Assert.Equal(0, gate.CurrentCount);
                    throw failure;
                },
                _ => throw new InvalidOperationException("Id must not run"),
                _ => throw new InvalidOperationException("StorePaths must not run"),
                new JsonSerializerOptions(), CancellationToken.None));
        Assert.Same(failure, error);
        Assert.Equal(1, gate.CurrentCount);
    }

    [Fact]
    public async Task IdentifierUsesPreparedConfigurationBeforeAnyPathOrSerialization()
    {
        using var gate = new SemaphoreSlim(1, 1);
        var prepared = new Document(Guid.NewGuid(), "prepared");
        var failure = new InvalidOperationException("Id failure");
        var calls = new List<string>();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(gate, "unused", new Document(Guid.Empty, "original"),
                value =>
                {
                    calls.Add(value.Value);
                    Assert.Equal(0, gate.CurrentCount);
                    return prepared;
                },
                value =>
                {
                    calls.Add(value.Value);
                    Assert.Same(prepared, value);
                    throw failure;
                },
                _ => throw new InvalidOperationException("StorePaths must not run"),
                new JsonSerializerOptions(), CancellationToken.None));
        Assert.Same(failure, error);
        Assert.Equal(["original", "prepared"], calls);
        Assert.Equal(1, gate.CurrentCount);
    }

    private sealed record Document(Guid Id, string Value);
}
