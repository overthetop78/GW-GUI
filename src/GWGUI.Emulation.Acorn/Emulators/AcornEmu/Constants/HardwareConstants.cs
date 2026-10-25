namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Valeurs matérielles du premier modèle ; elles sont distinctes des réglages audio/vidéo de l'hôte.</summary>
internal static class HardwareConstants
{
    internal const int CpuFrequency = 2_000_000;
    internal const int RamSize = 16_384;
    internal const int RomSize = 16_384;
    internal const int CyclesPerLine = 128;
    internal const int LinesPerFrame = 312;
    internal const int CyclesPerFrame = CyclesPerLine * LinesPerFrame;
    internal const int Width = 640;
    internal const int Height = 256;
}
