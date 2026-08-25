namespace GWGUI.Emulation.Atari.Contracts;


public sealed record AtariFirmwareConfiguration(
    AtariFirmwareCategory Category,
    string Path,
    bool IsRequired,
    bool IsOriginalFirmware = true);
