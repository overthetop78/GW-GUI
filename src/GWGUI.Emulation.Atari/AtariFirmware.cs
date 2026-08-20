namespace GWGUI.Emulation.Atari;

public enum AtariFirmwareKind
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
    AtariFirmwareKind Kind,
    string Path,
    bool IsRequired,
    bool IsOriginalFirmware = true);
