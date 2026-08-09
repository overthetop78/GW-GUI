using GWGUI.Scp.Images;

namespace GWGUI.Scp.SectorImages;

internal sealed class IbmPcIsoScpSectorImagePolicy(bool explicitlySelected) : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = ["iso.fm", "iso.mfm"];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var candidates = candidateSet.Addressed;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var cylinders = measured.Cylinders;
        var heads = measured.Heads;
        var sectorsPerTrack = measured.SectorsPerTrack;
        if (measured.SectorSize == 512 && !measured.ZeroBased)
        {
            var boot = IsoSectorImageBuilder.BestData(candidates, new(0, 0, 1));
            var fat = IsoSectorImageBuilder.BestData(candidates, new(0, 0, 2));
            var fatMedia = fat.Length > 0 ? fat[0] : (byte)0;
            var identified = explicitlySelected
                ? IbmPcImageReader.TryDetectFluxGeometry(boot, fatMedia, out var geometry)
                : IbmPcImageReader.TryIdentifyFluxGeometry(boot, fatMedia, out geometry);
            if (identified)
            {
                cylinders = geometry.Cylinders;
                heads = geometry.Heads;
                sectorsPerTrack = geometry.SectorsPerTrack;
            }
        }
        var resolved = IbmPcImageReader.FormatIdForGeometry(cylinders, heads, sectorsPerTrack, measured.SectorSize);
        return IsoSectorImageBuilder.CreateUniform(resolved, candidates, measured.SectorSize, cylinders, heads,
            sectorsPerTrack, address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
