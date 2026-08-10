using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Présélectionne les conteneurs CPCEMU DSK et applique leur interprétation Amstrad.</summary>
/// <param name="reader">Lecteur du conteneur CPCEMU DSK.</param>
internal sealed class AmstradContainerPolicy(CpcDskReader reader) : IDiskImageContainerPolicy
{
    /// <summary>Indique si l’extension peut désigner un conteneur CPCEMU DSK.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns><see langword="true"/> pour une extension DSK ou EDSK ; sinon <see langword="false"/>.</returns>
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase) ||
                             context.Extension.Equals(DiskImageFileExtensions.Edsk, StringComparison.OrdinalIgnoreCase));

    /// <summary>Lit le conteneur puis lui attribue l’interprétation CPC ou PCW correspondant à sa géométrie.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle interprétée.</returns>
    public async Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        var image = await reader.ReadAsync(context.Path, cancellationToken).ConfigureAwait(false);
        var formatId = image.Cylinders >= 80 && image.Heads == 2
            ? DiskImageFormatIds.AmstradPcw
            : DiskImageFormatIds.AmstradCpc;
        return SectorImageInterpretation.Retag(image, formatId);
    }
}
