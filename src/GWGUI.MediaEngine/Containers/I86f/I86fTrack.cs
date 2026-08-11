namespace GWGUI.MediaEngine.Containers.I86f;

public sealed record I86fTrack(int LogicalIndex, ushort Flags, int BitCount, IReadOnlyList<bool> Bits);
