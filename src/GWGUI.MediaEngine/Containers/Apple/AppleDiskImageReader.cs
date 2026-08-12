using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple;

/// <summary>Lit une image Apple en laissant le routeur distinguer les signatures certaines des simples indices de format.</summary>
public sealed class AppleDiskImageReader
{
    /// <summary>Charge une fois le fichier, puis valide et reconstruit son conteneur ou sa représentation Apple.</summary>
    /// <param name="path">Chemin du fichier Apple à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle entièrement validée.</returns>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return await ReadAsync(bytes, Path.GetExtension(path), null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Valide un contenu déjà chargé sans relire son fichier d'origine.</summary>
    /// <param name="bytes">Contenu complet de l'image.</param>
    /// <param name="extension">Extension servant uniquement d'indice aux formats sans signature.</param>
    /// <param name="requestedFormatId">Identifiant demandé, ou <see langword="null"/>.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le routage.</param>
    /// <returns>Image sectorielle entièrement validée.</returns>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, string extension, string? requestedFormatId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AppleContainerRouter.Read(bytes.ToArray(), extension, requestedFormatId));
    }
}
