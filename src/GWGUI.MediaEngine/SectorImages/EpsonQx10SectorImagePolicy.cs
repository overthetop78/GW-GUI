using GWGUI.MediaEngine.Geometries.Epson;

namespace GWGUI.MediaEngine.SectorImages;

internal static class EpsonQx10SectorImagePolicy
{
    public static SectorImage CreateImage(string formatId, IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates) => EpsonQx10SectorImageBuilder.Create(formatId, candidates);

    public static bool TryDetectFormat(IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates, out string formatId)
    {
        var sectors = candidates.Select(pair => new EpsonQx10SectorDescriptor(pair.Key.Cylinder, pair.Key.Head, pair.Key.Number, pair.Value.Where(value => value.Sector.Data is not null).GroupBy(value => value.Sector.IntegrityValid == true ? 2 : value.Sector.IntegrityValid is null ? 1 : 0).OrderByDescending(group => group.Key).First().GroupBy(value => value.Sector.Data!.Count).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key)).ToArray();
        return EpsonQx10FormatDetector.TryDetect(sectors, out formatId);
    }
}
