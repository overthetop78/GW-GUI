namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public enum GreaseweazleBusType : byte
{
    Invalid = 0,
    IbmPc = 1,
    Shugart = 2
}

public enum GreaseweazleCommand : byte
{
    GetInfo = 0,
    Seek = 2,
    Head = 3,
    Motor = 6,
    WriteFlux = 8,
    GetFluxStatus = 9,
    Select = 12,
    Deselect = 13,
    SetBusType = 14,
    Reset = 16
}

public enum GreaseweazleAcknowledgement : byte
{
    Okay = 0,
    BadCommand = 1,
    NoIndex = 2,
    NoTrackZero = 3,
    FluxOverflow = 4,
    FluxUnderflow = 5,
    WriteProtected = 6,
    NoUnit = 7,
    NoBus = 8,
    BadUnit = 9,
    BadPin = 10,
    BadCylinder = 11,
    OutOfSram = 12,
    OutOfFlash = 13
}

public sealed record GreaseweazleFirmwareInfo(
    byte MajorVersion,
    byte MinorVersion,
    byte MaximumCommand,
    uint SampleFrequency,
    byte HardwareModel,
    byte HardwareSubmodel,
    byte UsbSpeed,
    ushort MicrocontrollerId,
    ushort MicrocontrollerFrequencyMhz,
    ushort MicrocontrollerSramKib,
    byte UsbBufferKib,
    bool IsMainFirmware)
{
    public Version Version => new(MajorVersion, MinorVersion);
}

public sealed class GreaseweazleProtocolException(
    GreaseweazleCommand command,
    GreaseweazleAcknowledgement acknowledgement)
    : IOException($"Greaseweazle command {command} failed: {acknowledgement}.")
{
    public GreaseweazleCommand Command { get; } = command;

    public GreaseweazleAcknowledgement Acknowledgement { get; } = acknowledgement;
}
