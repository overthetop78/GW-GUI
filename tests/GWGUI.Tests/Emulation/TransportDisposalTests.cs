using GWGUI.Emulation.Functions;
using System.IO;

namespace GWGUI.Tests.Emulation;

public sealed class TransportDisposalTests
{
    [Fact]
    public void MissingTransportNeedsNoDisposal()
        => EmulationHostProtocolFunctions.DisposeTransport(null);

    [Fact]
    public void MissingQuietTransportNeedsNoDisposal()
        => EmulationHostProtocolFunctions.DisposeTransportIgnoringErrors(null);

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
        EmulationHostProtocolFunctions.DisposeTransportIgnoringErrors(transport);
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
            var actual = Record.Exception(() => EmulationHostProtocolFunctions.DisposeTransport(transport));
            Assert.Same(error, actual);
        }
        else
        {
            EmulationHostProtocolFunctions.DisposeTransport(transport);
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
