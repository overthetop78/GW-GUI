using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>Oriente les captures SCP Apple vers le reconstructeur Apple II, Macintosh/Lisa ou RWTS18.</summary>
public sealed class AppleScpSectorImageReader
{
    /// <summary>Lecteur du conteneur SCP partagé par les reconstructeurs Apple.</summary>
    private readonly IScpReader _scpReader;
    /// <summary>Reconstructeur des formats sectoriels Apple II DOS et ProDOS.</summary>
    private readonly AppleIIScpSectorReconstructor _appleII;
    /// <summary>Reconstructeur des formats Macintosh et Lisa à géométrie zonée.</summary>
    private readonly AppleMacScpSectorReconstructor _macintosh;
    /// <summary>Reconstructeur du format Apple II RWTS18.</summary>
    private readonly AppleRwts18ScpSectorReconstructor _rwts18;

    /// <summary>Crée le lecteur et ses reconstructeurs Apple spécialisés.</summary>
    /// <param name="scpReader">Lecteur utilisé pour analyser le conteneur SCP.</param>
    /// <param name="decoders">Registre fournissant les décodeurs de flux Apple.</param>
    public AppleScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
    {
        _scpReader = scpReader;
        var sectorDecoder = new AppleScpSectorDecoder(decoders);
        _appleII = new(sectorDecoder);
        _macintosh = new(sectorDecoder);
        _rwts18 = new(sectorDecoder);
    }

    /// <summary>Lit puis reconstruit une capture SCP Apple dans le format demandé ou détecté.</summary>
    /// <param name="path">Chemin de la capture SCP à reconstruire.</param>
    /// <param name="formatId">Identifiant Apple demandé, ou <see langword="null"/> pour essayer automatiquement les trois reconstructeurs.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la reconstruction.</param>
    /// <returns>L'image sectorielle Apple explicitement demandée ou la reconstruction automatique la plus complète.</returns>
    /// <exception cref="InvalidDataException">Le format demandé ne peut pas être reconstruit, ou les trois reconstructeurs automatiques ont rejeté la capture.</exception>
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await _scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase) == true)
            return _rwts18.Decode(scp, cancellationToken);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIAppleDosPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleIINoFileSystemPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleIIDosPrefix, StringComparison.OrdinalIgnoreCase) == true)
            return _appleII.Decode(scp, false, cancellationToken);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase) == true)
            return _appleII.Decode(scp, true, cancellationToken);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) == true)
            return _macintosh.Decode(scp, formatId, cancellationToken);

        return DetectAutomatically(scp, cancellationToken);
    }

    /// <summary>Essaie chaque famille Apple et conserve l'image la plus complète.</summary>
    /// <param name="scp">Capture SCP déjà analysée.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler les tentatives de reconstruction.</param>
    /// <returns>La reconstruction réussie possédant la meilleure proportion de blocs disponibles.</returns>
    /// <exception cref="InvalidDataException">Les reconstructeurs Macintosh/Lisa, Apple II et RWTS18 ont tous rejeté la capture.</exception>
    private SectorImage DetectAutomatically(ScpImage scp, CancellationToken cancellationToken)
    {
        var candidates = new List<SectorImage>(3);
        var rejections = new List<(string Identity, InvalidDataException Error)>(3);
        TryAdd(candidates, rejections, AppleScpReconstructionDefinitions.MacintoshReconstructorName, () => _macintosh.Decode(scp, null, cancellationToken));
        TryAdd(candidates, rejections, AppleScpReconstructionDefinitions.AppleIIReconstructorName, () => _appleII.Decode(scp, false, cancellationToken));
        TryAdd(candidates, rejections, AppleScpReconstructionDefinitions.Rwts18ReconstructorName, () => _rwts18.Decode(scp, cancellationToken));
        if (candidates.Count == 0) throw ScpReconstructionExceptions.AppleCandidatesRejected(rejections);
        return candidates.OrderByDescending(image => image.AvailableBlocks.Count / (double)Math.Max(1, image.BlockCount)).ThenByDescending(image => image.AvailableBlocks.Count).First();
    }

    /// <summary>Ajoute une reconstruction réussie sans interrompre la détection lorsqu'un encodage est absent.</summary>
    /// <param name="candidates">Images reconstruites avec succès.</param>
    /// <param name="rejections">Rejets déjà conservés avec l'identité de leur reconstructeur.</param>
    /// <param name="identity">Identité stable du reconstructeur essayé.</param>
    /// <param name="decode">Fonction exécutant la tentative de reconstruction.</param>
    private static void TryAdd(ICollection<SectorImage> candidates, ICollection<(string Identity, InvalidDataException Error)> rejections, string identity, Func<SectorImage> decode)
    {
        try { candidates.Add(decode()); }
        catch (InvalidDataException error) { rejections.Add((identity, error)); } // Cet encodage Apple est absent ; la détection automatique essaie le reconstructeur suivant.
    }
}
