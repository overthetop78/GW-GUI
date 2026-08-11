namespace GWGUI.MediaEngine.SectorImages;

public sealed record SectorBlock(
    int LogicalBlock,
    SectorAddress Address,
    IReadOnlyList<byte> Data,
    bool? IntegrityValid = true,
    int Revolution = 0,
    IReadOnlyList<byte>? Tag = null);
