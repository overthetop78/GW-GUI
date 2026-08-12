using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Recognition.Apple;

/// <summary>Examine les indices d'extension, de capacité et de structure des images Apple brutes sans lire de fichier.</summary>
internal static class AppleRawImageProbe
{
    /// <summary>Indique si le contenu correspond à une représentation brute Apple prise en charge.</summary>
    /// <param name="extension">Extension utilisée uniquement comme indice.</param>
    /// <param name="bytes">Contenu déjà chargé de l'image candidate.</param>
    /// <param name="requestedFormatId">Identifiant éventuellement demandé par le consommateur.</param>
    /// <returns><see langword="true"/> lorsque les indices propres à l'extension et au contenu sont cohérents.</returns>
    public static bool LooksLikeAppleImage(string extension, ReadOnlyMemory<byte> bytes, string? requestedFormatId)
    {
        _ = requestedFormatId;
        if (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase))
            return AppleDiskImageSignatures.LooksLikeLisaOfficePayload(bytes.Span) || MacintoshGcrGeometry.IsSupportedCapacity(bytes.Length) && AppleDiskImageSignatures.LooksLikeMac(bytes.Span);
        if (!extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
        return bytes.Length == AppleIIGeometry.Capacity || MacintoshGcrGeometry.IsSupportedCapacity(bytes.Length) && AppleDiskImageSignatures.LooksLikeMac(bytes.Span);
    }
}
