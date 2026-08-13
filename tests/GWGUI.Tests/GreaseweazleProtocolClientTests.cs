using System.Buffers.Binary;
using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.Tests;

public sealed class GreaseweazleProtocolClientTests
{
    [Fact]
    public async Task ClientNegotiatesAndRunsACompleteWriteSession()
    {
        var transport = new DeterministicTransport();
        await using var client = new GreaseweazleProtocolClient(transport);

        var firmware = await client.OpenAsync("COM7");
        await client.SetBusTypeAsync(GreaseweazleBusType.Shugart);
        await client.SelectDriveAsync(1);
        await client.SetMotorAsync(true);
        await client.SeekAsync(42, 1);
        await client.WriteFluxAsync(new uint[] { 100, 300, 50000 }, cueAtIndex: true, terminateAtIndex: true);
        await client.CloseAsync();

        Assert.Equal(new Version(1, 6), firmware.Version);
        Assert.Equal(72_000_000u, firmware.SampleFrequency);
        Assert.Equal("COM7", transport.PortName);
        Assert.Equal([9600, 10000, 9600], transport.BaudRates);
        Assert.True(transport.BuffersDiscarded);
        Assert.Equal(
            [
                GreaseweazleCommand.GetInfo,
                GreaseweazleCommand.SetBusType,
                GreaseweazleCommand.Select,
                GreaseweazleCommand.Motor,
                GreaseweazleCommand.Seek,
                GreaseweazleCommand.Head,
                GreaseweazleCommand.WriteFlux,
                GreaseweazleCommand.GetFluxStatus,
                GreaseweazleCommand.Motor,
                GreaseweazleCommand.Deselect
            ],
            transport.Commands);
        Assert.NotNull(transport.FluxStream);
        Assert.Equal(0, transport.FluxStream[^1]);
        Assert.False(transport.IsOpen);
    }

    [Fact]
    public async Task ClientReportsControllerAcknowledgements()
    {
        var transport = new DeterministicTransport
        {
            Failure = (GreaseweazleCommand.Seek, GreaseweazleAcknowledgement.BadCylinder)
        };
        await using var client = new GreaseweazleProtocolClient(transport);
        await client.OpenAsync("COM3");
        await client.SelectDriveAsync(0);

        var exception = await Assert.ThrowsAsync<GreaseweazleProtocolException>(async () =>
            await client.SeekAsync(90, 0));

        Assert.Equal(GreaseweazleCommand.Seek, exception.Command);
        Assert.Equal(GreaseweazleAcknowledgement.BadCylinder, exception.Acknowledgement);
    }

    [Fact]
    public async Task CancellationDoesNotPreventSafeDriveShutdown()
    {
        var transport = new DeterministicTransport();
        await using var client = new GreaseweazleProtocolClient(transport);
        await client.OpenAsync("COM4");
        await client.SelectDriveAsync(0);
        await client.SetMotorAsync(true);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await client.WriteFluxAsync(new uint[] { 100 }, true, true, cancellationToken: cancellation.Token));
        await client.CloseAsync();

        Assert.Equal(GreaseweazleCommand.Motor, transport.Commands[^2]);
        Assert.Equal(GreaseweazleCommand.Deselect, transport.Commands[^1]);
        Assert.False(transport.IsOpen);
    }

    [Fact]
    public async Task ClientReadsIndexedFluxAndPreservesTheRawStream()
    {
        var stream = new byte[] { 10, 255, 1, 181, 1, 1, 1, 120, 255, 1, 141, 1, 1, 1, 0 };
        var transport = new DeterministicTransport { ReadFluxStream = stream };
        await using var client = new GreaseweazleProtocolClient(transport);
        await client.OpenAsync("COM5");
        await client.SelectDriveAsync(0);
        await client.SetMotorAsync(true);

        var capture = await client.ReadFluxAsync(1);

        Assert.Equal(new uint[] { 10, 120 }, capture.FluxIntervals);
        Assert.Equal(new uint[] { 100, 100 }, capture.IndexIntervals);
        Assert.Equal(stream, capture.RawStream);
        Assert.Equal(72_000_000u, capture.SampleFrequency);
        Assert.Contains(GreaseweazleCommand.ReadFlux, transport.Commands);
        Assert.Contains(GreaseweazleCommand.GetFluxStatus, transport.Commands);
    }

    [Fact]
    public async Task ReadFluxRetriesTransientControllerOverflow()
    {
        var transport = new DeterministicTransport { ReadFluxStream = [10, 0] };
        transport.FluxStatusAcknowledgements.Enqueue(GreaseweazleAcknowledgement.FluxOverflow);
        transport.FluxStatusAcknowledgements.Enqueue(GreaseweazleAcknowledgement.Okay);
        await using var client = new GreaseweazleProtocolClient(transport);
        await client.OpenAsync("COM6");
        await client.SelectDriveAsync(0);
        await client.SetMotorAsync(true);

        var capture = await client.ReadFluxAsync(0, 1_000, retries: 1);

        Assert.Equal(new uint[] { 10 }, capture.FluxIntervals);
        Assert.Equal(2, transport.Commands.Count(command => command == GreaseweazleCommand.ReadFlux));
    }

    private sealed class DeterministicTransport : IGreaseweazleSerialTransport
    {
        private readonly Queue<byte> _readBytes = new();
        private bool _expectFlux;

        public bool IsOpen { get; private set; }
        public string? PortName { get; private set; }
        public List<int> BaudRates { get; } = [];
        public List<GreaseweazleCommand> Commands { get; } = [];
        public bool BuffersDiscarded { get; private set; }
        public byte[]? FluxStream { get; private set; }
        public byte[]? ReadFluxStream { get; init; }
        public Queue<GreaseweazleAcknowledgement> FluxStatusAcknowledgements { get; } = new();
        public (GreaseweazleCommand Command, GreaseweazleAcknowledgement Acknowledgement)? Failure { get; init; }

        public ValueTask OpenAsync(string portName, int baudRate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsOpen = true;
            PortName = portName;
            BaudRates.Add(baudRate);
            return ValueTask.CompletedTask;
        }

        public ValueTask SetBaudRateAsync(int baudRate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BaudRates.Add(baudRate);
            return ValueTask.CompletedTask;
        }

        public ValueTask DiscardBuffersAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BuffersDiscarded = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_expectFlux)
            {
                FluxStream = buffer.ToArray();
                _readBytes.Enqueue(0);
                _expectFlux = false;
                return ValueTask.CompletedTask;
            }

            var command = (GreaseweazleCommand)buffer.Span[0];
            Commands.Add(command);
            var acknowledgement = Failure is { } failure && failure.Command == command
                ? failure.Acknowledgement
                : GreaseweazleAcknowledgement.Okay;
            if (command == GreaseweazleCommand.GetFluxStatus && FluxStatusAcknowledgements.Count > 0)
                acknowledgement = FluxStatusAcknowledgements.Dequeue();
            _readBytes.Enqueue((byte)command);
            _readBytes.Enqueue((byte)acknowledgement);
            if (acknowledgement != GreaseweazleAcknowledgement.Okay) return ValueTask.CompletedTask;

            if (command == GreaseweazleCommand.GetInfo) EnqueueFirmware();
            if (command == GreaseweazleCommand.WriteFlux) _expectFlux = true;
            if (command == GreaseweazleCommand.ReadFlux)
            {
                foreach (var value in ReadFluxStream ?? [0]) _readBytes.Enqueue(value);
            }
            return ValueTask.CompletedTask;
        }

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = Math.Min(buffer.Length, _readBytes.Count);
            for (var index = 0; index < count; index++) buffer.Span[index] = _readBytes.Dequeue();
            return ValueTask.FromResult(count);
        }

        public ValueTask ReadExactlyAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var index = 0; index < buffer.Length; index++) buffer.Span[index] = _readBytes.Dequeue();
            return ValueTask.CompletedTask;
        }

        public ValueTask CloseAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsOpen = false;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            IsOpen = false;
            return ValueTask.CompletedTask;
        }

        private void EnqueueFirmware()
        {
            var response = new byte[32];
            response[0] = 1;
            response[1] = 6;
            response[2] = 1;
            response[3] = 22;
            BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(4), 72_000_000);
            response[8] = 7;
            response[9] = 1;
            response[10] = 1;
            response[11] = 64;
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(12), 4);
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(14), 144);
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(16), 224);
            foreach (var value in response) _readBytes.Enqueue(value);
        }
    }
}
