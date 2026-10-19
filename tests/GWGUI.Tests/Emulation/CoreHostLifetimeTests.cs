using System.IO;
using System.Text;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreHostLifetimeTests
{
    [Fact]
    public void MissingPreviousCoreNeedsNoDisposal()
    {
        Core? slot = null;
        EmulationHostProtocolFunctions.DisposePreviousCore(ref slot);
        Assert.Null(slot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReinitializationDetachesBeforeDisposalAndClearsTheReentrantSlot(bool fail)
    {
        var replacement = new Core();
        var error = new InvalidOperationException("disposal failure");
        Core? slot = new Core();
        var previous = slot;
        previous.OnDispose = () =>
        {
            Assert.Null(slot);
            slot = replacement;
            if (fail) throw error;
        };

        if (fail)
            Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
                EmulationHostProtocolFunctions.DisposePreviousCore(ref slot)));
        else
            EmulationHostProtocolFunctions.DisposePreviousCore(ref slot);

        Assert.Null(slot);
        Assert.Equal(1, previous.DisposeCount);
        Assert.Equal(0, replacement.DisposeCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisposeCommandAndSessionFinallyReleaseTheOwnerOnlyOnce(bool fail)
    {
        var expected = new InvalidOperationException("teardown failure");
        var owned = new Core();
        owned.OnDispose = () => { if (fail) throw expected; };
        Core? slot = owned;
        using var input = new MemoryStream([(byte)Command.Dispose, (byte)Command.Dispose]);
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
        try
        {
            EmulationHostProtocolFunctions.RunCommandLoop<Command>(
                reader, writer, static request => (Command)request.ReadByte(), Command.Dispose,
                (_, response) =>
                {
                    EmulationHostProtocolFunctions.DisposePreviousCore(ref slot);
                    response.Write(true);
                },
                (response, error) =>
                {
                    Assert.Same(expected, error);
                    response.Write(false);
                    response.Write(error.ToString());
                });
        }
        finally
        {
            EmulationHostProtocolFunctions.DisposePreviousCore(ref slot);
        }

        Assert.Null(slot);
        Assert.Equal(1, owned.DisposeCount);
        Assert.Equal(1, input.Position);
        Assert.True(input.CanRead);
        Assert.True(output.CanWrite);
        writer.Flush();
        output.Position = 0;
        using var transport = new BinaryReader(output, Encoding.UTF8, leaveOpen: true);
        var bytes = EmulationHostProtocolFunctions.ReadBytes(transport, "test");
        using var payload = new MemoryStream(bytes, writable: false);
        using var responseReader = new BinaryReader(payload, Encoding.UTF8, leaveOpen: true);
        Assert.Equal(!fail, responseReader.ReadBoolean());
        if (fail) Assert.Contains(expected.Message, responseReader.ReadString());
        Assert.Equal(payload.Length, payload.Position);
        Assert.Equal(output.Length, output.Position);
        writer.Write((byte)42);
    }

    private enum Command : byte
    {
        Dispose = 11
    }

    private sealed class Core : IDisposable
    {
        internal Action? OnDispose { get; set; }
        internal int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            OnDispose?.Invoke();
        }
    }
}
