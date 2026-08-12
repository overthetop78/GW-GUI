using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Sélectionne automatiquement la politique sectorielle adaptée aux candidats ISO FM ou MFM.</summary>
internal sealed class AutomaticIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiants des deux décodeurs ISO essayés pendant la reconnaissance automatique.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    /// <summary>Mesure les candidats puis les oriente vers la reconstruction Epson, BBC, IBM, Atari 8 bits ou Atari ST correspondante.</summary>
    /// <param name="formatId">Identifiant inutilisé lors de la sélection automatique.</param>
    /// <param name="candidates">Candidats ISO regroupés par adresse logique et physique.</param>
    /// <returns>L'image sectorielle construite par la première famille dont les critères sont satisfaits.</returns>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidates)
    {
        if (EpsonQx10SectorImagePolicy.TryDetectFormat(candidates.Physical, out var epsonFormat))
            return EpsonQx10SectorImagePolicy.CreateImage(epsonFormat, candidates.Physical);

        var measured = IsoSectorImageBuilder.Measure(candidates.Addressed);
        if (measured.ZeroBased && measured.SectorSize == 256 && measured.SectorsPerTrack == 10)
            return new BbcIsoScpSectorImagePolicy().Build(null, candidates);
        if (measured.SectorSize == 512 && !measured.ZeroBased)
        {
            var boot = IsoSectorImageBuilder.BestData(candidates.Addressed, new(0, 0, 1));
            var fat = IsoSectorImageBuilder.BestData(candidates.Addressed, new(0, 0, 2));
            var fatMedia = fat.Length > 0 ? fat[0] : (byte)0;
            if (GWGUI.MediaEngine.Recognition.Ibm.IbmDosDiskProbe.TryIdentify(boot, fatMedia, true, out _))
                return new IbmPcIsoScpSectorImagePolicy(false).Build(null, candidates);
        }

        var atari8Bit = measured.SectorSize is 128 or 256 && measured.Heads == DiskGeometryConstants.SingleSidedHeadCount && measured.SectorsPerTrack is 18 or 26;
        return atari8Bit ? new Atari8BitIsoScpSectorImagePolicy(null).Build(null, candidates) : new AtariStIsoScpSectorImagePolicy().Build(null, candidates);
    }
}
