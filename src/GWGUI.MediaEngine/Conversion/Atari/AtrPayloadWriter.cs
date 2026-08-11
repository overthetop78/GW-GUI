using GWGUI.MediaEngine.Containers.Atari.Atr;

namespace GWGUI.MediaEngine.Conversion.Atari;

/// <summary>Extrait la charge utile sectorielle d'un conteneur ATR validé.</summary>
public static class AtrPayloadWriter
{
    /// <summary>Écrit la charge utile ATR sans son en-tête dans un fichier brut.</summary>
    /// <param name="sourcePath">Chemin du conteneur ATR source.</param>
    /// <param name="destinationPath">Chemin du fichier sectoriel brut à produire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture ou l'écriture.</param>
    /// <returns>Tâche représentant l'extraction asynchrone.</returns>
    /// <exception cref="ArgumentException">Un chemin est vide, ne contient que des espaces ou possède un format non pris en charge.</exception>
    /// <exception cref="FileNotFoundException">Le conteneur ATR source est introuvable.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès au fichier source ou à la destination est refusé.</exception>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture ou l'écriture.</exception>
    /// <exception cref="InvalidDataException">Le conteneur source ne respecte pas la disposition ATR attendue.</exception>
    /// <exception cref="OperationCanceledException">Le jeton d'annulation demande l'arrêt de l'opération.</exception>
    public static async Task WriteRawPayloadAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        var data = await AtrReader.ReadValidatedContainerAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await File.WriteAllBytesAsync(destinationPath, data.AsMemory(AtrLayout.HeaderSize), cancellationToken).ConfigureAwait(false);
    }
}
