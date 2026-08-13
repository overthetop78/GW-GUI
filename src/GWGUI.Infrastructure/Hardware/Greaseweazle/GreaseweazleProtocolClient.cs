using System.Buffers.Binary;

namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public sealed class GreaseweazleProtocolClient(
    IGreaseweazleSerialTransport transport,
    int maximumFluxUnderflowRetries = 5) : IGreaseweazleWriteDevice
{
    public const int CommunicationClearBaudRate = 10000;
    public const int NormalBaudRate = 9600;
    public static readonly Version EarliestSupportedFirmware = new(0, 31);

    private bool _selected;
    private bool _motorOn;
    private byte _selectedUnit;

    public GreaseweazleFirmwareInfo? Firmware { get; private set; }

    public async ValueTask<GreaseweazleFirmwareInfo> OpenAsync(
        string portName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portName);
        await transport.OpenAsync(portName, NormalBaudRate, cancellationToken);
        try
        {
            await ResetCommunicationAsync(cancellationToken);
            await SendCommandAsync(new byte[] { 0, 3, 0 }, cancellationToken);
            var response = new byte[32];
            await transport.ReadExactlyAsync(response, cancellationToken);
            Firmware = ParseFirmware(response);
            if (!Firmware.IsMainFirmware)
                throw new InvalidOperationException("The Greaseweazle controller is in firmware-update mode.");
            if (Firmware.Version < EarliestSupportedFirmware)
                throw new InvalidOperationException($"Greaseweazle firmware {Firmware.Version} is too old.");
            return Firmware;
        }
        catch
        {
            await transport.CloseAsync(CancellationToken.None);
            throw;
        }
    }

    public ValueTask SetBusTypeAsync(
        GreaseweazleBusType busType,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(new byte[] { (byte)GreaseweazleCommand.SetBusType, 3, (byte)busType }, cancellationToken);

    public async ValueTask SelectDriveAsync(byte unit, CancellationToken cancellationToken = default)
    {
        await SendCommandAsync(new byte[] { (byte)GreaseweazleCommand.Select, 3, unit }, cancellationToken);
        _selected = true;
        _selectedUnit = unit;
    }

    public async ValueTask SetMotorAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        EnsureDriveSelected();
        await SendCommandAsync(
            new byte[] { (byte)GreaseweazleCommand.Motor, 4, _selectedUnit, enabled ? (byte)1 : (byte)0 },
            cancellationToken);
        _motorOn = enabled;
    }

    public async ValueTask SeekAsync(
        short cylinder,
        byte head,
        CancellationToken cancellationToken = default)
    {
        EnsureDriveSelected();
        if (cylinder is >= sbyte.MinValue and <= sbyte.MaxValue)
        {
            await SendCommandAsync(
                new byte[] { (byte)GreaseweazleCommand.Seek, 3, unchecked((byte)(sbyte)cylinder) },
                cancellationToken);
        }
        else
        {
            var command = new byte[4];
            command[0] = (byte)GreaseweazleCommand.Seek;
            command[1] = 4;
            BinaryPrimitives.WriteInt16LittleEndian(command.AsSpan(2), cylinder);
            await SendCommandAsync(command, cancellationToken);
        }

        await SendCommandAsync(new byte[] { (byte)GreaseweazleCommand.Head, 3, head }, cancellationToken);
    }

    public async ValueTask WriteFluxAsync(
        ReadOnlyMemory<uint> intervals,
        bool cueAtIndex,
        bool terminateAtIndex,
        uint hardSectorTicks = 0,
        CancellationToken cancellationToken = default)
    {
        EnsureDriveSelected();
        if (!_motorOn) throw new InvalidOperationException("The drive motor is not running.");
        if (Firmware is null) throw new InvalidOperationException("The controller is not open.");
        var stream = GreaseweazleFluxStreamEncoder.Encode(intervals.Span, Firmware.SampleFrequency);

        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await SendCommandAsync(BuildWriteCommand(cueAtIndex, terminateAtIndex, hardSectorTicks), cancellationToken);
                await transport.WriteAsync(stream, cancellationToken);
                var synchronization = new byte[1];
                await transport.ReadExactlyAsync(synchronization, cancellationToken);
                await SendCommandAsync(new byte[] { (byte)GreaseweazleCommand.GetFluxStatus, 2 }, cancellationToken);
                return;
            }
            catch (GreaseweazleProtocolException exception) when (
                exception.Acknowledgement == GreaseweazleAcknowledgement.FluxUnderflow &&
                attempt < maximumFluxUnderflowRetries)
            {
            }
        }
    }

    public async ValueTask ResetAsync(CancellationToken cancellationToken = default)
    {
        await SendCommandAsync(new byte[] { (byte)GreaseweazleCommand.Reset, 2 }, cancellationToken);
        _selected = false;
        _motorOn = false;
    }

    public async ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        if (!transport.IsOpen) return;
        try
        {
            try
            {
                if (_motorOn)
                {
                    await SendCommandAsync(
                        new byte[] { (byte)GreaseweazleCommand.Motor, 4, _selectedUnit, 0 },
                        CancellationToken.None);
                }
            }
            finally
            {
                if (_selected)
                    await SendCommandAsync(new byte[] { (byte)GreaseweazleCommand.Deselect, 2 }, CancellationToken.None);
            }
        }
        finally
        {
            _motorOn = false;
            _selected = false;
            Firmware = null;
            await transport.CloseAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync(CancellationToken.None);
        await transport.DisposeAsync();
    }

    private async ValueTask ResetCommunicationAsync(CancellationToken cancellationToken)
    {
        await transport.SetBaudRateAsync(CommunicationClearBaudRate, cancellationToken);
        await transport.SetBaudRateAsync(NormalBaudRate, cancellationToken);
        await transport.DiscardBuffersAsync(cancellationToken);
    }

    private async ValueTask SendCommandAsync(ReadOnlyMemory<byte> command, CancellationToken cancellationToken)
    {
        if (!transport.IsOpen) throw new InvalidOperationException("The Greaseweazle transport is closed.");
        await transport.WriteAsync(command, cancellationToken);
        var response = new byte[2];
        await transport.ReadExactlyAsync(response, cancellationToken);
        if (response[0] != command.Span[0])
            throw new IOException($"Unexpected Greaseweazle command response {response[0]}.");
        var acknowledgement = (GreaseweazleAcknowledgement)response[1];
        if (acknowledgement != GreaseweazleAcknowledgement.Okay)
            throw new GreaseweazleProtocolException((GreaseweazleCommand)command.Span[0], acknowledgement);
    }

    private static GreaseweazleFirmwareInfo ParseFirmware(ReadOnlySpan<byte> response)
    {
        if (response.Length != 32) throw new InvalidDataException("The firmware response must contain 32 bytes.");
        return new(
            response[0],
            response[1],
            response[3],
            BinaryPrimitives.ReadUInt32LittleEndian(response[4..8]),
            response[8],
            response[9],
            response[10],
            BinaryPrimitives.ReadUInt16LittleEndian(response[12..14]),
            BinaryPrimitives.ReadUInt16LittleEndian(response[14..16]),
            BinaryPrimitives.ReadUInt16LittleEndian(response[16..18]),
            response[11],
            response[2] != 0);
    }

    private static byte[] BuildWriteCommand(bool cueAtIndex, bool terminateAtIndex, uint hardSectorTicks)
    {
        if (hardSectorTicks == 0)
            return [(byte)GreaseweazleCommand.WriteFlux, 4, cueAtIndex ? (byte)1 : (byte)0, terminateAtIndex ? (byte)1 : (byte)0];

        var command = new byte[8];
        command[0] = (byte)GreaseweazleCommand.WriteFlux;
        command[1] = 8;
        command[2] = cueAtIndex ? (byte)1 : (byte)0;
        command[3] = terminateAtIndex ? (byte)1 : (byte)0;
        BinaryPrimitives.WriteUInt32LittleEndian(command.AsSpan(4), hardSectorTicks);
        return command;
    }

    private void EnsureDriveSelected()
    {
        if (!_selected) throw new InvalidOperationException("No Greaseweazle drive is selected.");
    }
}
