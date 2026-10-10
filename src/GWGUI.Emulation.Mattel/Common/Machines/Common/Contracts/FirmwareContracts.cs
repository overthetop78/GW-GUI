using System.IO;
namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(FirmwareCategory Category, string Path,
    bool IsRequired, bool IsOriginalFirmware = true);
