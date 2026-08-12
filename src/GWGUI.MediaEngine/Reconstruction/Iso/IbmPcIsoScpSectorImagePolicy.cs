using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Recognition.Ibm;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Construit une image IBM PC depuis des candidats ISO FM ou MFM.</summary>
/// <param name="explicitlySelected">Indique si la géométrie IBM a été demandée explicitement sans exiger un OEM DOS connu.</param>
internal sealed class IbmPcIsoScpSectorImagePolicy(bool explicitlySelected) : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiants des décodeurs ISO FM et MFM acceptés par la politique IBM.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    /// <summary>Mesure les candidats, affine leur géométrie avec le BPB ou la FAT puis construit l'image IBM.</summary>
    /// <param name="formatId">Identifiant demandé ; la géométrie détectée détermine l'identifiant final.</param>
    /// <param name="candidateSet">Candidats ISO regroupés par adresse logique et physique.</param>
    /// <returns>L'image sectorielle IBM associée à la géométrie mesurée ou détectée.</returns>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var candidates = candidateSet.Addressed;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var cylinders = measured.Cylinders;
        var heads = measured.Heads;
        var sectorsPerTrack = measured.SectorsPerTrack;
        if (measured.SectorSize == FatBpbLayout.SectorSize && !measured.ZeroBased)
        {
            var boot = IsoSectorImageBuilder.BestData(candidates, new(FatBpbLayout.SystemCylinder, FatBpbLayout.SystemHead, FatBpbLayout.BootSectorNumber));
            var fat = IsoSectorImageBuilder.BestData(candidates, new(FatBpbLayout.SystemCylinder, FatBpbLayout.SystemHead, FatBpbLayout.FirstFatSectorNumber));
            var fatMedia = fat.Length > FatBpbLayout.FatMediaDescriptorDataOffset ? fat[FatBpbLayout.FatMediaDescriptorDataOffset] : FatBpbLayout.UnknownMediaDescriptor;
            var identified = explicitlySelected ? IbmBootGeometryDetector.TryDetect(boot, fatMedia, out var geometry) : IbmDosDiskProbe.TryIdentify(boot, fatMedia, true, out geometry);
            if (identified)
            {
                cylinders = geometry.Cylinders;
                heads = geometry.Heads;
                sectorsPerTrack = geometry.SectorsPerTrack;
            }
        }
        var resolved = IbmPcGeometryCatalog.FormatIdForGeometry(cylinders, heads, sectorsPerTrack, measured.SectorSize);
        return IsoSectorImageBuilder.CreateUniform(resolved, candidates, measured.SectorSize, cylinders, heads, sectorsPerTrack, address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
