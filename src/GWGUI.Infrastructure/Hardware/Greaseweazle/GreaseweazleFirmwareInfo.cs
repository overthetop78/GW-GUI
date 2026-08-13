namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

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
