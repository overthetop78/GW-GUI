namespace GWGUI.Scp.SectorImages;

internal sealed class AutomaticIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = ["iso.fm", "iso.mfm"];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidates)
    {
        if (EpsonQx10SectorImagePolicy.TryDetectFormat(candidates.Physical, out var epsonFormat))
            return EpsonQx10SectorImagePolicy.CreateImage(epsonFormat, candidates.Physical);

        var measured = IsoSectorImageBuilder.Measure(candidates.Addressed);
        if (measured.SectorSize == 512 && !measured.ZeroBased)
        {
            var boot = IsoSectorImageBuilder.BestData(candidates.Addressed, new(0, 0, 1));
            var fat = IsoSectorImageBuilder.BestData(candidates.Addressed, new(0, 0, 2));
            var fatMedia = fat.Length > 0 ? fat[0] : (byte)0;
            if (GWGUI.Scp.Images.IbmPcImageReader.TryIdentifyFluxGeometry(boot, fatMedia, out _))
                return new IbmPcIsoScpSectorImagePolicy(false).Build(null, candidates);
        }

        return new AtariIsoScpSectorImagePolicy(null).Build(null, candidates);
    }
}
