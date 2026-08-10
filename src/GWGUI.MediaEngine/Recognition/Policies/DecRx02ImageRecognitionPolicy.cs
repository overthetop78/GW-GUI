using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne la géométrie physique RX02 puis vérifie séparément la structure RT-11 attendue.</summary>
/// <param name="reader">Lecteur chargé de remettre les secteurs physiques RX02 en ordre logique.</param>
internal sealed class DecRx02ImageRecognitionPolicy(DecRx02ImageReader reader) : IDiskImageRecognitionPolicy
{
    /// <summary>Vérifie d'abord la capacité RX02, puis le format demandé ou le home block RT-11.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du contenu.</param>
    /// <returns><see langword="true"/> pour une géométrie RX02 explicitement demandée ou contenant une structure RT-11 crédible.</returns>
    public async ValueTask<bool> CanReadAsync(
        DiskImageRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Length != DecRx02ImageReader.ImageSize) return false;
        if (context.RequestedFormatId?.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase) == true)
            return true;
        return DecRx02ImageReader.LooksLikeRt11(
            await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Lit le dump physique RX02 présélectionné et produit ses blocs logiques de 512 octets.</summary>
    /// <param name="context">Contexte du dump RX02.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle DEC RX02 remise en ordre logique.</returns>
    /// <exception cref="InvalidDataException">La taille du dump RX02 est incomplète.</exception>
    public Task<SectorImage> ReadAsync(
        DiskImageRecognitionContext context,
        CancellationToken cancellationToken) => reader.ReadAsync(context.Path, cancellationToken);
}
