namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(FirmwareCategory Category, string Path, bool IsRequired, bool IsOriginalFirmware = true);


