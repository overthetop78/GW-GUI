namespace GWGUI.Emulation.Fairchild.Common.Contracts;

internal sealed record DiskImageStatus(int Index, string? Path, string? Label);

internal sealed record DiskStatus(int ImageCount, int CurrentIndex, bool IsEjected, IReadOnlyList<DiskImageStatus> Images);

