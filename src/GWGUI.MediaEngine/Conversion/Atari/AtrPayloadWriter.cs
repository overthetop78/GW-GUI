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
    /// <exception cref="InvalidDataException">Le conteneur source ne respecte pas la disposition ATR attendue.</exception>
    public static async Task WriteRawPayloadAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        var data = await AtrReader.ReadValidatedContainerAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await File.WriteAllBytesAsync(destinationPath, data.AsMemory(AtrLayout.HeaderSize), cancellationToken).ConfigureAwait(false);
    }
}
