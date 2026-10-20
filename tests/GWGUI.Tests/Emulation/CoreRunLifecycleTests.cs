using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreRunLifecycleTests
{
    [Fact]
    public void SuccessfulRunCleansUpAfterExecutionWithInitializationCompleted()
    {
        var calls = new List<string>();
        ExternalCoreLifecycleFunctions.RunCore(
            () => calls.Add("initialize"), () => calls.Add("run"),
            _ => () => throw new InvalidOperationException("Unexpected report"),
            initialized => calls.Add($"cleanup:{initialized}"));

        Assert.Equal(new[] { "initialize", "run", "cleanup:True" }, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReportsOriginalFailureAndCleansUpWithActualInitializationState(bool failInitialize)
    {
        var calls = new List<string>();
        var failure = new InvalidOperationException("original");
        Exception? reported = null;
        bool? cleanupState = null;
        ExternalCoreLifecycleFunctions.RunCore(
            () => { calls.Add("initialize"); if (failInitialize) throw failure; },
            () => { calls.Add("run"); throw failure; },
            error => () => { calls.Add("report"); reported = error; },
            initialized => { calls.Add("cleanup"); cleanupState = initialized; });

        Assert.Same(failure, reported);
        Assert.Equal(!failInitialize, cleanupState);
        Assert.Equal(failInitialize
            ? new[] { "initialize", "report", "cleanup" }
            : new[] { "initialize", "run", "report", "cleanup" }, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReportFailureStillRunsCleanupBeforePropagating(bool failInitialize)
    {
        var reportFailure = new ApplicationException("report");
        bool? cleanupState = null;
        var actual = Assert.Throws<ApplicationException>(() => ExternalCoreLifecycleFunctions.RunCore(
            () => { if (failInitialize) throw new InvalidOperationException("initialize"); },
            () => throw new InvalidOperationException("run"),
            _ => () => throw reportFailure,
            initialized => cleanupState = initialized));

        Assert.Same(reportFailure, actual);
        Assert.Equal(!failInitialize, cleanupState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CleanupFailureTakesPrecedenceAndIsNotReportedAgain(bool failInitialize)
    {
        var cleanupFailure = new ApplicationException("cleanup");
        var reports = 0;
        var actual = Assert.Throws<ApplicationException>(() => ExternalCoreLifecycleFunctions.RunCore(
            () => { if (failInitialize) throw new InvalidOperationException("initialize"); },
            () => throw new InvalidOperationException("run"),
            _ => () => { reports++; throw new InvalidOperationException("report"); },
            _ => throw cleanupFailure));

        Assert.Same(cleanupFailure, actual);
        Assert.Equal(1, reports);
    }

    [Fact]
    public void PassesCancellationToTheOriginalTypedHandlerWithoutWrappingIt()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var failure = new OperationCanceledException(cancellation.Token);
        Exception? reported = null;
        bool? cleanupState = null;
        ExternalCoreLifecycleFunctions.RunCore(() => throw failure,
            () => throw new InvalidOperationException("Run must not start"),
            error => () => reported = error,
            initialized => cleanupState = initialized);

        Assert.Same(failure, reported);
        Assert.False(cleanupState);
    }

    [Fact]
    public void SelectsHandlerBeforeUsingUnwindsAndReportsAfterUnwind()
    {
        var calls = new List<string>();
        var scopeDisposed = false;
        bool? observedAtSelection = null;
        ExternalCoreLifecycleFunctions.RunCore(
            () => calls.Add("initialize"),
            () =>
            {
                using var scope = new Scope(() => { scopeDisposed = true; calls.Add("dispose"); });
                calls.Add("run");
                throw new InvalidOperationException("run");
            },
            _ =>
            {
                observedAtSelection = scopeDisposed;
                calls.Add("select");
                return () => { Assert.True(scopeDisposed); calls.Add("report"); };
            },
            _ => calls.Add("cleanup"));

        Assert.False(observedAtSelection);
        Assert.Equal(new[] { "initialize", "run", "select", "dispose", "report", "cleanup" }, calls);
    }

    private sealed class Scope(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
