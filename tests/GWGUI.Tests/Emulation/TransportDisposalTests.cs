using GWGUI.Emulation;
using GWGUI.Emulation.Functions;
using System.IO;

namespace GWGUI.Tests.Emulation;

public sealed class TransportDisposalTests
{
    [Theory]
    [InlineData("none")]
    [InlineData("io")]
    [InlineData("disposed")]
    [InlineData("unexpected")]
    [InlineData("canceled")]
    public void CoreDisposalReturnsSuccessOnlyAfterDisposalAndReportsTheOriginalFailure(string failure)
    {
        Exception? error = failure switch
        {
            "io" => new IOException("Core closed"),
            "disposed" => new ObjectDisposedException("core"),
            "unexpected" => new InvalidOperationException("Core failed"),
            "canceled" => new OperationCanceledException(),
            _ => null
        };
        var core = new Transport(error);
        var reported = new List<Exception>();
        var released = EmulationSdk.MachineExecution.TryDisposeCore(core, reported.Add);

        Assert.Equal(error is null, released);
        Assert.Equal(1, core.DisposeCount);
        if (error is null) Assert.Empty(reported);
        else Assert.Same(error, Assert.Single(reported));
    }

    [Fact]
    public void FailureReporterErrorsStillEscapeBeforeTheReleaseFlagCanBeSet()
    {
        var core = new Transport(new IOException("Dispose failed"));
        var reportFailure = new InvalidOperationException("Reporter failed");
        var released = false;
        var finalizationReached = false;

        var actual = Assert.Throws<InvalidOperationException>(() =>
        {
            try { released = EmulationSdk.MachineExecution.TryDisposeCore(core, _ => throw reportFailure); }
            finally { finalizationReached = true; }
        });

        Assert.Same(reportFailure, actual);
        Assert.False(released);
        Assert.True(finalizationReached);
        Assert.Equal(1, core.DisposeCount);
    }

    [Fact]
    public void MissingCorePreservesTheOriginalCaughtDereferenceFailure()
    {
        var reported = new List<Exception>();
        Assert.False(EmulationSdk.MachineExecution.TryDisposeCore(null!, reported.Add));
        Assert.IsType<NullReferenceException>(Assert.Single(reported));
    }

    [Fact]
    public void MissingTransportNeedsNoDisposal()
        => EmulationSdk.HostProtocol.DisposeTransport(null);

    [Fact]
    public void MissingQuietTransportNeedsNoDisposal()
        => EmulationSdk.HostProtocol.DisposeTransportIgnoringErrors(null);

    [Theory]
    [InlineData("none")]
    [InlineData("io")]
    [InlineData("disposed")]
    [InlineData("unexpected")]
    [InlineData("canceled")]
    public void QuietLegacyDisposalPreservesItsAllExceptionPolicy(string failure)
    {
        Exception? error = failure switch
        {
            "io" => new IOException("Transport closed"),
            "disposed" => new ObjectDisposedException("transport"),
            "unexpected" => new InvalidOperationException("Unexpected failure"),
            "canceled" => new OperationCanceledException(),
            _ => null
        };
        var transport = new Transport(error);
        EmulationSdk.HostProtocol.DisposeTransportIgnoringErrors(transport);
        Assert.Equal(1, transport.DisposeCount);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("io")]
    [InlineData("disposed")]
    [InlineData("unexpected")]
    [InlineData("canceled")]
    public void DisposalPreservesTheTransportExceptionPolicy(string failure)
    {
        Exception? error = failure switch
        {
            "io" => new IOException("Transport closed"),
            "disposed" => new ObjectDisposedException("transport"),
            "unexpected" => new InvalidOperationException("Unexpected failure"),
            "canceled" => new OperationCanceledException(),
            _ => null
        };
        var transport = new Transport(error);
        if (failure is "unexpected" or "canceled")
        {
            var actual = Record.Exception(() => EmulationSdk.HostProtocol.DisposeTransport(transport));
            Assert.Same(error, actual);
        }
        else
        {
            EmulationSdk.HostProtocol.DisposeTransport(transport);
        }
        Assert.Equal(1, transport.DisposeCount);
    }

    private sealed class Transport(Exception? error) : IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            if (error is not null) throw error;
        }
    }
}
