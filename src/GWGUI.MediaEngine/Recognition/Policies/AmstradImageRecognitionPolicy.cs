using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Images.Containers;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Reconnaît les signatures CPCEMU DSK puis applique l'interprétation Amstrad correspondant à la géométrie lue.</summary>
/// <param name="reader">Lecteur neutre des conteneurs CPCEMU DSK.</param>
internal sealed class AmstradImageRecognitionPolicy(CpcDskReader reader) : IDiskImageRecognitionPolicy
{
    /// <summary>Recherche une signature CPCEMU DSK Standard ou Extended au début du contenu, indépendamment de l'extension.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns><see langword="true"/> lorsque le contenu commence par une signature CPCEMU reconnue ; sinon <see langword="false"/>.</returns>
    public async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return bytes.Span.StartsWith(CpcDskFormat.StandardSignatureBytes) || bytes.Span.StartsWith(CpcDskFormat.ExtendedSignatureBytes);
    }

    /// <summary>Lit le conteneur neutre puis lui attribue l'identifiant CPC ou PCW déterminé par sa géométrie.</summary>
    /// <param name="context">Contexte du conteneur CPCEMU à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle dont les secteurs restent inchangés et dont l'identifiant décrit l'interprétation Amstrad.</returns>
    /// <exception cref="InvalidDataException">Le parser CPCEMU rejette la structure ou les données sectorielles du conteneur.</exception>
    public async Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        var image = await reader.ReadAsync(context.Path, cancellationToken).ConfigureAwait(false);
        var formatId = image.Cylinders >= DiskGeometryConstants.EightyTrackCylinderCount && image.Heads == DiskGeometryConstants.DoubleSidedHeadCount
            ? DiskImageFormatIds.AmstradPcw
            : DiskImageFormatIds.AmstradCpc;
        return SectorImageInterpretation.Retag(image, formatId);
    }

}
