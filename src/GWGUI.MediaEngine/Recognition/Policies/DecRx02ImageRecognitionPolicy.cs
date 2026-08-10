using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne la géométrie physique RX02 puis vérifie séparément la structure RT-11 attendue.</summary>
/// <param name="reader">Lecteur chargé de remettre les secteurs physiques RX02 en ordre logique.</param>
internal sealed class DecRx02ImageRecognitionPolicy(DecRx02ImageReader reader) : ReaderBackedRecognitionPolicy(reader.ReadAsync)
{
    /// <summary>Vérifie d'abord la capacité RX02, puis le format demandé ou le home block RT-11.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du contenu.</param>
    /// <returns><see langword="true"/> pour une géométrie RX02 explicitement demandée ou contenant une structure RT-11 crédible.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.Length != DecRx02ImageReader.ImageSize) return false;
        if (context.RequestedFormatId?.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase) == true)
            return true;
        return DecRx02ImageReader.LooksLikeRt11(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));
    }
}
