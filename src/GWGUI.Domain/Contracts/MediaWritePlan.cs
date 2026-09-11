using GWGUI.Domain.Enums;

namespace GWGUI.Domain.Contracts;

/// <summary>Describes neutral data units and constraints to be consumed by a physical media writer.</summary>
public sealed record MediaWritePlan(
    MediaKind MediaKind,
    IReadOnlyList<ReadOnlyMemory<byte>> DataUnits,
    IReadOnlyList<int> WriteOrder,
    IReadOnlyDictionary<string, string> Constraints);
