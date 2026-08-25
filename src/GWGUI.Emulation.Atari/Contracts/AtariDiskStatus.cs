namespace GWGUI.Emulation.Atari.Contracts;


internal sealed record AtariDiskStatus(
    int ImageCount,
    int CurrentIndex,
    bool IsEjected,
    IReadOnlyList<AtariDiskImageStatus> Images);
