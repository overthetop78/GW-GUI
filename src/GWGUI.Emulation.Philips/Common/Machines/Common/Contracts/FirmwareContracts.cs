namespace GWGUI.Emulation.Philips.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(FirmwareCategory Category, string Path,
    bool IsRequired, bool IsOriginalFirmware = true);
