namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Définit les types de disquettes publics du champ Disk Type SCP.</summary>
public enum ScpDiskType : byte
{
    Amiga = 0x04,
    AmigaHighDensity = 0x08,
    Atari8BitSingleDensity = 0x10,
    Atari8BitDoubleDensity = 0x11,
    Atari8BitEnhancedDensity = 0x12,
    AtariStSingleSided = 0x14,
    AtariStDoubleSided = 0x15,
    AppleII = 0x20,
    AppleIIProDos = 0x21,
    AppleMacintosh400 = 0x24,
    AppleMacintosh800 = 0x25,
    AppleMacintosh1440 = 0x26,
    IbmPc360 = 0x30,
    IbmPc720 = 0x31,
    IbmPc1200 = 0x32,
    IbmPc1440 = 0x33,
    AmstradCpc = 0x70,
    Other320 = 0x80,
    Other1200 = 0x81,
    Other720 = 0x84,
    Other1440 = 0x85
}
