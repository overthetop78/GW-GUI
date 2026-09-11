using GWGUI.Domain.Enums;

namespace GWGUI.Domain.Contracts;

/// <summary>Contains neutral data acquired from a physical medium and the diagnostics reported by its reader.</summary>
public sealed record MediaAcquisitionResult(
    MediaKind MediaKind,
    MediaRepresentationKind RepresentationKind,
    IReadOnlyList<ReadOnlyMemory<byte>> DataUnits,
    IReadOnlyList<string> HardwareDiagnostics);
