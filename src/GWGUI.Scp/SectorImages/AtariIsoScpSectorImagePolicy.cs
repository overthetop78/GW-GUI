namespace GWGUI.Scp.SectorImages;

internal sealed class AtariIsoScpSectorImagePolicy(string? requestedFormatId) : IIsoScpSectorImagePolicy
{
    private readonly bool atari8 = requestedFormatId?.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) == true;
    private readonly bool atariSt = requestedFormatId?.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) == true;

    public IReadOnlyList<string> DecoderIds { get; } = requestedFormatId switch
    {
        "atari.90" => ["iso.fm"],
        not null when requestedFormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) => ["iso.mfm"],
        not null when requestedFormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) => ["iso.mfm"],
        _ => ["iso.fm", "iso.mfm"]
    };

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var candidates = candidateSet.Addressed;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var sectorSize = measured.SectorSize;
        var cylinders = measured.Cylinders;
        var heads = measured.Heads;
        var sectorsPerTrack = measured.SectorsPerTrack;
        var zeroBased = measured.ZeroBased;
        var is8Bit = atari8 || (!atariSt && sectorSize is 128 or 256 && heads == 1 && sectorsPerTrack is 18 or 26);
        var resolvedFormat = formatId ?? (zeroBased && sectorSize == 256 && sectorsPerTrack == 10
            ? heads == 1 ? cylinders == 40 ? "acorn.dfs.ss" : "acorn.dfs.ss80" : cylinders == 40 ? "acorn.dfs.ds" : "acorn.dfs.ds80"
            : is8Bit
                ? (sectorSize, sectorsPerTrack) switch
                {
                    (128, 18) => "atari.90",
                    (128, 26) => "atari.130",
                    (256, 18) => "atari.180",
                    _ => $"atari.scp.{sectorSize}.{sectorsPerTrack}"
                }
                : $"atarist.{(cylinders * heads * sectorsPerTrack * sectorSize) / 1024}");
        var capacity = is8Bit && sectorSize > 128
            ? 3L * 128 + (cylinders * heads * sectorsPerTrack - 3L) * sectorSize
            : (long)cylinders * heads * sectorsPerTrack * sectorSize;
        return IsoSectorImageBuilder.CreateUniform(resolvedFormat, candidates, sectorSize, cylinders, heads,
            sectorsPerTrack, address => zeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1,
            allowVariableBlockSize: is8Bit && sectorSize > 128, capacity: capacity);
    }
}
