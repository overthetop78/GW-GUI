using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Recognition.Ibm;

namespace GWGUI.MediaEngine.SectorImages;

internal sealed class IbmPcIsoScpSectorImagePolicy(bool explicitlySelected) : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

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
                ? IbmDosDiskProbe.TryIdentify(boot, fatMedia, false, out var geometry)
                : IbmDosDiskProbe.TryIdentify(boot, fatMedia, true, out geometry);
            if (identified)
            {
                cylinders = geometry.Cylinders;
                heads = geometry.Heads;
                sectorsPerTrack = geometry.SectorsPerTrack;
            }
        }
        var resolved = IbmPcGeometryCatalog.FormatIdForGeometry(cylinders, heads, sectorsPerTrack, measured.SectorSize);
        return IsoSectorImageBuilder.CreateUniform(resolved, candidates, measured.SectorSize, cylinders, heads,
            sectorsPerTrack, address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
