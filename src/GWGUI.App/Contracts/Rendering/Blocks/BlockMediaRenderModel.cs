using GWGUI.App.Enums.Rendering.Blocks;

namespace GWGUI.App.Contracts.Rendering.Blocks;

public sealed record BlockMediaRenderModel(
    long LogicalLength,
    IReadOnlyList<BlockMediaRange> Ranges,
    BlockMediaGeometry? Geometry = null);

public sealed record BlockMediaRange(
    long Start,
    long Length,
    BlockMediaRangeState State,
    string? PartitionTable = null,
    int? PartitionNumber = null,
    string? FileSystemId = null);

public sealed record BlockMediaGeometry(
    long Cylinders,
    int Heads,
    int SectorsPerTrack,
    int? PlatterCount = null);
