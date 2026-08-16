namespace GWGUI.Emulation.Atari.Cores;

internal sealed record AtariDiskImageStatus(int Index, string? Path, string? Label);

internal sealed record AtariDiskStatus(
    int ImageCount,
    int CurrentIndex,
    bool IsEjected,
    IReadOnlyList<AtariDiskImageStatus> Images);
