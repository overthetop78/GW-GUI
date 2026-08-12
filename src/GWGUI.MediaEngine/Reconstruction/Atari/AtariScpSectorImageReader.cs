using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Reconstruction.Atari;

/// <summary>Reconstruit les images sectorielles Atari 8 bits et Atari ST depuis un flux ISO FM ou MFM.</summary>
/// <param name="scpReader">Lecteur utilisé pour analyser le conteneur SCP.</param>
/// <param name="decoders">Registre fournissant les décodeurs de flux ISO.</param>
public sealed class AtariScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    /// <summary>Lecteur ISO commun auquel la reconstruction Atari est déléguée.</summary>
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    /// <summary>Valide l'identifiant Atari demandé puis reconstruit la capture avec la politique ISO correspondante.</summary>
    /// <param name="path">Chemin de la capture SCP à reconstruire.</param>
    /// <param name="formatId">Identifiant Atari demandé, ou <see langword="null"/> pour une sélection automatique.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la reconstruction.</param>
    /// <returns>L'image sectorielle Atari 8 bits ou Atari ST reconstruite.</returns>
    /// <exception cref="ArgumentException"><paramref name="formatId"/> n'appartient ni à la famille Atari 8 bits ni à la famille Atari ST.</exception>
    public Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        if (formatId is not null &&
            !formatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) &&
            !formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase))
            throw AtariScpReconstructionExceptions.UnsupportedFormat(formatId, nameof(formatId));

        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
