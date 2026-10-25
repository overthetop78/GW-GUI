using System.Buffers.Binary;
using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.Tests.Hardware;

public class GreaseweazlePositionTests
{
    [Theory]
    [InlineData(0, 0)] [InlineData(1, 1)] [InlineData(300, 1)]
    [InlineData(-1, 0)] [InlineData(-1, 1)]
    public async Task ConsistentPositionSelectsHead(int cylinder, int pin)
    {
        var transport = new Transport([(byte)pin]);
        var client = new GreaseweazleProtocolClient(transport);
        try
        {
            await client.OpenAsync("virtual");
            await client.SelectDriveAsync(0);
            await client.SeekAsync((short)cylinder, 1);
            Assert.Contains(transport.Commands, c => c.SequenceEqual(new byte[] { 20, 3, 26 }));
            Assert.Equal(new byte[] { 3, 3, 1 }, transport.Commands[^1]);
        }
        finally { await client.DisposeAsync(); }
        Assert.False(transport.IsOpen);
    }

    [Theory]
    [InlineData(0, 1)] [InlineData(1, 0)] [InlineData(300, 0)]
    public async Task InconsistentPositionStopsBeforeHeadOrFlux(int cylinder, int pin)
    {
        var transport = new Transport([(byte)pin, (byte)pin]);
        var client = new GreaseweazleProtocolClient(transport);
        try
        {
            await client.OpenAsync("virtual");
            await client.SelectDriveAsync(0);
            var error = await Assert.ThrowsAsync<GreaseweazlePositionException>(() => client.SeekAsync((short)cylinder, 0).AsTask());
            Assert.Equal((short)cylinder, error.Cylinder);
            Assert.Equal(pin == 0, error.TrackZeroAsserted);
            Assert.DoesNotContain(transport.Commands, c => c[0] is 3 or 7 or 8 or 16 or 17);
        }
        finally { await client.DisposeAsync(); }
        Assert.False(transport.IsOpen);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task FlippyCorrectionMatchesSupportedCommands(int mode)
    {
        var transport = new Transport([1, mode == 0 ? (byte)0 : (byte)1])
        {
            Flippy = mode != 3,
            RejectDriveInfo = mode == 1,
            RejectNoClickStep = mode == 2
        };
        var client = new GreaseweazleProtocolClient(transport);
        try
        {
            await client.OpenAsync("virtual");
            await client.SelectDriveAsync(0);
            if (mode == 0) await client.SeekAsync(0, 0);
            else await Assert.ThrowsAsync<GreaseweazlePositionException>(() => client.SeekAsync(0, 0).AsTask());
            Assert.Equal(mode is 0 or 2, transport.Commands.Any(c => c[0] == 22));
            Assert.Equal(2, transport.Commands.Count(c => c[0] == 20));
            Assert.Equal(mode == 0, transport.Commands.Any(c => c[0] == 3));
            Assert.DoesNotContain(transport.Commands, c => c[0] is 7 or 8 or 16 or 17);
        }
        finally { await client.DisposeAsync(); }
        Assert.False(transport.IsOpen);
    }

    private sealed class Transport(byte[] pins) : IGreaseweazleSerialTransport
    {
        private readonly Queue<byte> pinValues = new(pins);
        public bool IsOpen { get; private set; }
        public bool Flippy, RejectDriveInfo, RejectNoClickStep;
        public List<byte[]> Commands { get; } = [];
        public ValueTask OpenAsync(string port, int baud, CancellationToken token = default) { IsOpen = true; return ValueTask.CompletedTask; }
        public ValueTask SetBaudRateAsync(int baud, CancellationToken token = default) => ValueTask.CompletedTask;
        public ValueTask DiscardBuffersAsync(CancellationToken token = default) => ValueTask.CompletedTask;
        public ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken token = default) { Commands.Add(bytes.ToArray()); return ValueTask.CompletedTask; }
        public ValueTask<int> ReadAsync(Memory<byte> bytes, CancellationToken token = default) => throw new InvalidOperationException("Flux reading was not requested.");
        public ValueTask ReadExactlyAsync(Memory<byte> bytes, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var command = Commands[^1];
            bytes.Span.Clear();
            if (bytes.Length == 1) bytes.Span[0] = pinValues.Dequeue();
            else if (bytes.Length == 2)
            {
                bytes.Span[0] = command[0];
                if ((command[0] == 0 && command[2] == 7 && RejectDriveInfo) || (command[0] == 22 && RejectNoClickStep)) bytes.Span[1] = 1;
            }
            else
            {
                Assert.Equal(32, bytes.Length);
                if (command[2] == 7) BinaryPrimitives.WriteUInt32LittleEndian(bytes.Span, Flippy ? 4u : 0u);
                else { bytes.Span[1] = 31; bytes.Span[2] = 1; BinaryPrimitives.WriteUInt32LittleEndian(bytes.Span[4..], 24000000); }
            }
            return ValueTask.CompletedTask;
        }
        public ValueTask CloseAsync(CancellationToken token = default) { IsOpen = false; return ValueTask.CompletedTask; }
        public ValueTask DisposeAsync() => CloseAsync();
    }
}
