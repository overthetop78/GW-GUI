namespace GWGUI.Scp.SectorImages;

internal sealed class Atari8BitIsoScpSectorImagePolicy(string? requestedFormatId) : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = requestedFormatId == "atari.90"
        ? ["iso.fm"] : ["iso.mfm"];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var candidates = candidateSet.Addressed;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var resolvedFormat = formatId ?? (measured.SectorSize, measured.SectorsPerTrack) switch
        {
            (128, 18) => "atari.90",
            (128, 26) => "atari.130",
            (256, 18) => "atari.180",
            _ => $"atari.scp.{measured.SectorSize}.{measured.SectorsPerTrack}"
        };
        var capacity = measured.SectorSize > 128
            ? 3L * 128 + (measured.Cylinders * measured.Heads * measured.SectorsPerTrack - 3L) * measured.SectorSize
            : (long)measured.Cylinders * measured.Heads * measured.SectorsPerTrack * measured.SectorSize;
        return IsoSectorImageBuilder.CreateUniform(resolvedFormat, candidates, measured.SectorSize,
            measured.Cylinders, measured.Heads, measured.SectorsPerTrack,
            address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1,
            allowVariableBlockSize: measured.SectorSize > 128, capacity: capacity);
    }
}
