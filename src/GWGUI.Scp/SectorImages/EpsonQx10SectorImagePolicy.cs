namespace GWGUI.Scp.SectorImages;

internal static class EpsonQx10SectorImagePolicy
{
    public static SectorImage CreateImage(
        string formatId,
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates) =>
        EpsonQx10SectorImageBuilder.Create(formatId, candidates);

    public static bool TryDetectFormat(
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates,
        out string formatId) =>
        EpsonQx10FormatDetector.TryDetect(candidates, out formatId);
}
