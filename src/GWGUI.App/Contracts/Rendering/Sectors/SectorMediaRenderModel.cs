using GWGUI.App.Enums.Rendering.Sectors;

namespace GWGUI.App.Contracts.Rendering.Sectors;

public sealed record SectorMediaRenderModel(
    string FormatId,
    int BlockSize,
    IReadOnlyList<SectorMediaSurface> Surfaces);

public sealed record SectorMediaSurface(
    int Index,
    IReadOnlyList<SectorMediaTrack> Tracks);

public sealed record SectorMediaTrack(
    int Cylinder,
    IReadOnlyList<SectorMediaElement> Sectors);

public sealed record SectorMediaElement(
    long Position,
    int LogicalBlock,
    int Cylinder,
    int Number,
    int Size,
    SectorMediaElementState State,
    string? FileSystemPath = null);
