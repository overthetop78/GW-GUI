using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Contracts;

/// <summary>A byte signature searched at a fixed position or in the indicated direction.</summary>
public sealed record MediaContentSignature(
    int? Position,
    MediaContentSearchDirection Direction,
    IReadOnlyList<byte> Bytes);
