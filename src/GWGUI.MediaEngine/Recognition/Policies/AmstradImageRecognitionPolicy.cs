using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Images.Containers;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Reconnaît les signatures CPCEMU DSK puis applique l'interprétation Amstrad correspondant à la géométrie lue.</summary>
/// <param name="reader">Lecteur neutre des conteneurs CPCEMU DSK.</param>
internal sealed class AmstradImageRecognitionPolicy(CpcDskReader reader) : IDiskImageContainerPolicy
{
    /// <summary>Recherche une signature CPCEMU DSK Standard ou Extended au début du contenu, indépendamment de l'extension.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns><see langword="true"/> lorsque le contenu commence par une signature CPCEMU reconnue ; sinon <see langword="false"/>.</returns>
    public async ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return StartsWith(bytes, CpcDskFormat.StandardSignature) ||
               StartsWith(bytes, CpcDskFormat.ExtendedSignature);
    }

    /// <summary>Lit le conteneur neutre puis lui attribue l'identifiant CPC ou PCW déterminé par sa géométrie.</summary>
    /// <param name="context">Contexte du conteneur CPCEMU à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle dont les secteurs restent inchangés et dont l'identifiant décrit l'interprétation Amstrad.</returns>
    /// <exception cref="InvalidDataException">Le parser CPCEMU rejette la structure ou les données sectorielles du conteneur.</exception>
    public async Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        var image = await reader.ReadAsync(context.Path, cancellationToken).ConfigureAwait(false);
        var formatId = image.Cylinders >= 80 && image.Heads == 2
            ? DiskImageFormatIds.AmstradPcw
            : DiskImageFormatIds.AmstradCpc;
        return SectorImageInterpretation.Retag(image, formatId);
    }

    /// <summary>Indique si les premiers octets correspondent à une signature ASCII donnée.</summary>
    /// <param name="bytes">Contenu du fichier.</param>
    /// <param name="signature">Signature CPCEMU attendue.</param>
    /// <returns><see langword="true"/> lorsque la signature est complète et identique.</returns>
    private static bool StartsWith(byte[] bytes, string signature) =>
        bytes.AsSpan().StartsWith(System.Text.Encoding.ASCII.GetBytes(signature));
}
