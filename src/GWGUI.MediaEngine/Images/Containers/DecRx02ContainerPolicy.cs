using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Reconnaît les dumps DEC RX02 explicitement demandés ou contenant une structure RT-11 crédible.</summary>
/// <param name="reader">Lecteur des dumps physiques DEC RX02.</param>
internal sealed class DecRx02ContainerPolicy(DecRx02ImageReader reader) : IDiskImageContainerPolicy
{
    /// <summary>Vérifie l’indice IMG, le format demandé et la structure interne RT-11.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de la lecture.</param>
    /// <returns><see langword="true"/> lorsque le fichier est un candidat RX02.</returns>
    public async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (!context.Extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase)) return false;
        return context.RequestedFormatId?.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase) == true ||
               DecRx02ImageReader.LooksLikeRt11(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Lit et remet en ordre les secteurs physiques du dump RX02.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle logique DEC RX02.</returns>
    public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
