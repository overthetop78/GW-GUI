namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(IReadOnlyDictionary<FirmwareCategory, string> Images);
