namespace GWGUI.Emulation.Atari;

public enum AtariFirmwareCategory
{
    Tos,
    AtariSystemOs,
    AtariOsA,
    AtariOsB,
    AtariXlOs,
    AtariBasic,
    Atari5200Bios,
    AtariXegsBios,
    Atari7800Bios,
    LynxBootRom,
    JaguarBootRom,
    JaguarCdBios
}

public sealed record AtariFirmwareConfiguration(
    AtariFirmwareCategory Category,
    string Path,
    bool IsRequired,
    bool IsOriginalFirmware = true);
