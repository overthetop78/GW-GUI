using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Contracts;

/// <summary>One or more byte groups matched at a fixed position or successively in the indicated direction.</summary>
public sealed record MediaContentSignature(
    int? Position,
    MediaContentSearchDirection Direction,
    IReadOnlyList<IReadOnlyList<byte>> ByteGroups);
