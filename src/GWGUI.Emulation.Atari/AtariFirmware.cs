namespace GWGUI.Emulation.Atari;

public enum AtariFirmwareKind
{
    Tos,
    AtariOsA,
    AtariOsB,
    AtariXlOs,
    AtariBasic,
    Atari5200Bios,
    AtariXegsBios,
    Atari7800Bios,
    LynxBootRom,
    JaguarCdBios
}

public sealed record AtariFirmwareConfiguration(
    AtariFirmwareKind Kind,
    string Path,
    bool IsRequired,
    bool IsOriginalFirmware = true);
