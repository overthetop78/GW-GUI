namespace GWGUI.Domain.Hardware;

public sealed record GwDeviceInfo(
    string? HostToolsVersion,
    string? Port,
    string? Model,
    string? Mcu,
    string? FirmwareVersion,
    string? SerialNumber,
    string? UsbSpeed,
    bool HasNetworkWarning,
    string RawOutput);
