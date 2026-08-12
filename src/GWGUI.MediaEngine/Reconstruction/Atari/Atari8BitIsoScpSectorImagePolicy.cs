using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Reconstruction.Atari;

/// <summary>Construit une image Atari 8 bits depuis des candidats ISO FM ou MFM.</summary>
/// <param name="requestedFormatId">Identifiant Atari demandé, utilisé pour sélectionner le décodeur FM du format 90 Kio ou le décodeur MFM des autres formats.</param>
internal sealed class Atari8BitIsoScpSectorImagePolicy(string? requestedFormatId) : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiant du décodeur ISO adapté au format Atari demandé.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = requestedFormatId == DiskImageFormatIds.Atari90 ? [FluxCodecIds.IsoFm] : [FluxCodecIds.IsoMfm];

    /// <summary>Mesure les candidats Atari et construit l'image en tenant compte des trois secteurs d'amorçage de 128 octets.</summary>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/> pour le déduire de la taille et du nombre de secteurs.</param>
    /// <param name="candidateSet">Candidats ISO regroupés et validés avant la construction.</param>
    /// <returns>L'image sectorielle Atari avec sa capacité logique exacte, exprimée en octets.</returns>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var candidates = candidateSet.Addressed;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var resolvedFormat = formatId ?? (measured.SectorSize, measured.SectorsPerTrack) switch
        {
            (128, 18) => DiskImageFormatIds.Atari90,
            (128, 26) => DiskImageFormatIds.Atari130,
            (256, 18) => DiskImageFormatIds.Atari180,
            _ => DiskImageFormatIds.AtariScp(measured.SectorSize, measured.SectorsPerTrack)
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
