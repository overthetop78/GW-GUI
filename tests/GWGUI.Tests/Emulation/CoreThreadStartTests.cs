using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreThreadStartTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StartsOwnedBackgroundWorkerWithLinkedTokenAndIndependentStartupSignal(bool canceled)
    {
        using var cancellation = new CancellationTokenSource();
        using var ready = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        if (canceled) cancellation.Cancel();
        CancellationToken workerToken = default;
        CancellationTokenSource? stop = null;
        Task? loop = null;
        Thread? thread = null;
        try
        {
            ExternalCoreLifecycleFunctions.StartThread(token =>
            {
                workerToken = token;
                ready.Set();
                release.Wait();
            }, "GW GUI common startup regression", cancellation.Token,
                out stop, out var started, out loop, out thread);

            Assert.True(ready.Wait(TimeSpan.FromSeconds(5)));
            Assert.NotNull(thread);
            Assert.True(thread.IsBackground);
            Assert.Equal("GW GUI common startup regression", thread.Name);
            Assert.NotNull(stop);
            Assert.Equal(stop.Token, workerToken);
            Assert.Equal(canceled, workerToken.IsCancellationRequested);
            Assert.NotNull(loop);
            Assert.False(loop.IsCompleted);
            Assert.False(started.Task.IsCompleted);
            cancellation.Cancel();
            Assert.True(workerToken.IsCancellationRequested);
        }
        finally
        {
            release.Set();
            try
            {
                if (loop is not null) await loop;
            }
            finally
            {
                try { thread?.Join(); }
                finally { stop?.Dispose(); }
            }
        }
        Assert.NotNull(thread);
        Assert.False(thread.IsAlive);
    }
}
