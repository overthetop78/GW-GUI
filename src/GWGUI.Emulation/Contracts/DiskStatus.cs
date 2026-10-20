namespace GWGUI.Emulation.Contracts;

internal sealed record DiskStatus(
    int ImageCount,
    int CurrentIndex,
    bool IsEjected,
    IReadOnlyList<DiskImageStatus> Images);
