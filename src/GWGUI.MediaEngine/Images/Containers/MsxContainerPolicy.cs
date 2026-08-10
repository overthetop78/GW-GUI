using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Reconnaît les images brutes MSX-DOS à partir de leur indice DSK et de leur BPB.</summary>
/// <param name="reader">Lecteur des images sectorielles MSX.</param>
internal sealed class MsxContainerPolicy(MsxImageReader reader) : IDiskImageContainerPolicy
{
    /// <summary>Vérifie l’indice DSK, le format demandé et les champs internes du BPB MSX.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de la lecture.</param>
    /// <returns><see langword="true"/> lorsque le fichier est un candidat MSX.</returns>
    public async ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        if (!context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
        return context.FormatId?.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) == true ||
               MsxImageReader.LooksLikeMsx(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Lit l’image MSX validée par la politique.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle MSX.</returns>
    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
